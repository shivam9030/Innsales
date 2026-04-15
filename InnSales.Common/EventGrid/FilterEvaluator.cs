
using System.Text.Json;

namespace MockEventGrid;

public static class FilterEvaluator
{
    public static bool MatchesAll(MockEventGridEvent ev, IReadOnlyList<AdvancedFilter> filters)
        => filters.All(f => Matches(ev, f));

    public static bool Matches(MockEventGridEvent ev, AdvancedFilter f)
    {
        if (string.Equals(f.Key, "eventType", StringComparison.OrdinalIgnoreCase))
            return EvalString(ev.EventType, f);
        if (string.Equals(f.Key, "subject", StringComparison.OrdinalIgnoreCase))
            return EvalString(ev.Subject, f);
        if (f.Key.StartsWith("data.", StringComparison.OrdinalIgnoreCase))
        {
            var path = f.Key.Substring("data.".Length);
            if (TryGetFromJson(ev.Data, path, out var value))
            {
                return value.ValueKind switch
                {
                    JsonValueKind.String => EvalString(value.GetString()!, f),
                    JsonValueKind.Number => EvalNumber(value.GetDouble(), f),
                    _ => false
                };
            }
            return false;
        }
        return false;
    }

    static bool EvalString(string v, AdvancedFilter f) => f.Operator switch
    {
        "StringEquals"     => f.Values?.Any(x => string.Equals(v, x, StringComparison.OrdinalIgnoreCase)) == true,
        "StringBeginsWith" => f.Values?.Any(x => v.StartsWith(x, StringComparison.OrdinalIgnoreCase)) == true,
        "StringEndsWith"   => f.Values?.Any(x => v.EndsWith(x, StringComparison.OrdinalIgnoreCase)) == true,
        "StringContains"   => f.Values?.Any(x => v.IndexOf(x, StringComparison.OrdinalIgnoreCase) >= 0) == true,
        _ => false
    };

    static bool EvalNumber(double n, AdvancedFilter f) => f.Operator switch
    {
        "NumberEquals"      => f.Values?.Any(x => double.TryParse(x, out var d) && n == d) == true,
        "NumberGreaterThan" => f.Values?.Any(x => double.TryParse(x, out var d) && n > d) == true,
        "NumberLessThan"    => f.Values?.Any(x => double.TryParse(x, out var d) && n < d) == true,
        "NumberIn"          => f.Values?.Any(x => double.TryParse(x, out var d) && Math.Abs(n - d) < double.Epsilon) == true,
        _ => false
    };

    static bool TryGetFromJson(JsonElement root, string path, out JsonElement value)
    {
        value = default;
        var segs = path.Split('.', StringSplitOptions.RemoveEmptyEntries);
        var cur = root;
        foreach (var s in segs)
        {
            if (cur.ValueKind != JsonValueKind.Object || !cur.TryGetProperty(s, out cur))
                return false;
        }
        value = cur;
        return true;
    }
}
