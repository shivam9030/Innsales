
using System.Text.Json;

namespace MockEventGrid;

public sealed record MockEventGridEvent(
    string Subject,
    string EventType,
    string DataVersion,
    JsonElement Data,
    DateTimeOffset EventTime);

public sealed record AdvancedFilter(
    string Key,            // e.g., "data.CustomerId" | "eventType" | "subject"
    string Operator,       // e.g., "NumberIn", "StringEquals", "StringBeginsWith"
    IReadOnlyList<string>? Values = null);

public sealed record EventSubscriptionDefinition(
    string Name,
    Uri Endpoint,                      // webhook target
    IReadOnlyList<AdvancedFilter> Filters,
    int MaxDeliveryAttempts = 10,      // retry budget
    TimeSpan? BaseRetryDelay = null,   // exponential backoff base
    bool BatchDelivery = false,        // deliver batches or single
    string? DeadLetterContainer = null // Azurite blob container name
);
