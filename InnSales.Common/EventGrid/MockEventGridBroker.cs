
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Azure.Storage.Blobs;

namespace MockEventGrid;

public sealed class MockEventGridBroker
{
    private readonly ConcurrentDictionary<string, List<EventSubscriptionDefinition>> _subs = new();
    private readonly HttpClient _http = new();
    private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);
    private readonly BlobServiceClient? _blobSvc;

    /// <param name="azuriteConnectionString">Use "UseDevelopmentStorage=true" for Azurite</param>
    public MockEventGridBroker(string? azuriteConnectionString = "UseDevelopmentStorage=true")
    {
        if (!string.IsNullOrWhiteSpace(azuriteConnectionString))
        {
            try { _blobSvc = new BlobServiceClient(azuriteConnectionString); } catch { _blobSvc = null; }
        }
    }

    public void CreateTopic(string topicName) => _subs.TryAdd(topicName, new List<EventSubscriptionDefinition>());

    public void CreateSubscription(string topicName, EventSubscriptionDefinition def)
    {
        var list = _subs.GetOrAdd(topicName, _ => new List<EventSubscriptionDefinition>());
        list.Add(def);
            Console.WriteLine(
        $"[MockEventGrid] Subscription created → Topic: {topicName}, Name: {def.Name}, Endpoint: {def.Endpoint}"
    );

    }

    public async Task PublishAsync(string topicName, IEnumerable<MockEventGridEvent> events, CancellationToken ct = default)
    {
        Console.WriteLine($"MockEventGridBroker: Publishing the event");
        if (!_subs.TryGetValue(topicName, out var subs) || subs.Count == 0) return;

        foreach (var sub in subs)
        {
            var matches = events.Where(e => FilterEvaluator.MatchesAll(e, sub.Filters)).ToList();
            Console.WriteLine("MockEventGridBroker: Found "+matches.Count+" matching events for subscription "+sub.Name);
            if (matches.Count == 0) continue;

            if (sub.BatchDelivery)
                await DeliverWithRetryAsync(sub, JsonSerializer.Serialize(matches, _json), ct);
            else
                foreach (var ev in matches)
                    await DeliverWithRetryAsync(sub, JsonSerializer.Serialize(new[] { ev }, _json), ct);
        }
    }

    private async Task DeliverWithRetryAsync(EventSubscriptionDefinition sub, string payload, CancellationToken ct)
    {
        var attempts = 0;
        var maxAttempts = Math.Max(sub.MaxDeliveryAttempts, 1);
        var baseDelay = sub.BaseRetryDelay ?? TimeSpan.FromSeconds(1);

        while (attempts < maxAttempts && !ct.IsCancellationRequested)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Post, sub.Endpoint)
                { Content = new StringContent(payload, Encoding.UTF8, "application/json") };
                 Console.WriteLine("MockEventGridBroker: Delivering event to " + sub.Endpoint);
                var res = await _http.SendAsync(req, ct);
                if ((int)res.StatusCode >= 200 && (int)res.StatusCode < 300)
                    return; // delivered
            }
            catch { /* retry */ }

            attempts++;
            var delay = TimeSpan.FromMilliseconds(baseDelay.TotalMilliseconds * Math.Pow(2, attempts));
            await Task.Delay(delay, ct);
        }

        // Dead-letter: container/SUBNAME-UPPER/YYYY/MM/DD/HH/<guid>.json (like Event Grid docs)
        if (!string.IsNullOrWhiteSpace(sub.DeadLetterContainer) && _blobSvc is not null)
        {
            var container = _blobSvc.GetBlobContainerClient(sub.DeadLetterContainer);
            await container.CreateIfNotExistsAsync(cancellationToken: ct);
            var path = $"{sub.Name.ToUpperInvariant()}/{DateTime.UtcNow:yyyy/MM/dd/HH}/{Guid.NewGuid()}.json";
            var blob = container.GetBlobClient(path);
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(payload));
            await blob.UploadAsync(ms, overwrite: false, cancellationToken: ct);
        }
    }
}
