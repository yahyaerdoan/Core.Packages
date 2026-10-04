using System.Reflection;
using Core.ApplicationLayer.Pipelines.Loggings.Abstractions;

namespace Core.ApplicationLayer.Pipelines.Loggings.Concretions;

internal static class RequestRedactor<TRequest>
{
    private const string RedactedValue = "***REDACTED***";

    private static readonly PropertyInfo[] Properties = typeof(TRequest).GetProperties(BindingFlags.Public | BindingFlags.Instance);

    private static readonly HashSet<PropertyInfo> SensitiveProperties = [.. Properties.Where(p => p.GetCustomAttribute<SensitiveDataAttribute>() is not null)];

    public static object Redact(TRequest request)
    {
        return request is null || SensitiveProperties.Count == 0
            ? (object?)request ?? string.Empty
            : Properties.ToDictionary(p => p.Name, p => SensitiveProperties.Contains(p) ? RedactedValue : p.GetValue(request));
    }
}
