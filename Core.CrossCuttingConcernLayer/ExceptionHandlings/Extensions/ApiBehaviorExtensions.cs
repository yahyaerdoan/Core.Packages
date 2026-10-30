using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using ResultHandler.AspNetCore.Extensions;
using ResultHandler.Core.Abstractions;
using ResultHandler.Core.Base;

namespace Core.CrossCuttingConcernLayer.ExceptionHandlings.Extensions;

public static partial class ApiBehaviorExtensions
{
    // Model-binding failures bypass MediatR and never throw, so route them through ResultHandler here.
    public static IServiceCollection AddConfigureCustomModelValidation(this IServiceCollection services)
    {
        _ = services.Configure<ApiBehaviorOptions>(options => options.InvalidModelStateResponseFactory = context =>
            {
                List<(string Key, ModelError Error)> entries = context.ModelState
                    .Where(entry => entry.Value?.Errors.Count > 0)
                    .SelectMany(entry => entry.Value!.Errors.Select(error => (entry.Key, Error: error)))
                    .ToList();

                // Prefer JSON-path-keyed entries ("$.modelId") over the vague parameter-name one.
                List<(string Key, ModelError Error)> jsonPathEntries = entries.Where(e => e.Key.StartsWith('$')).ToList();
                if (jsonPathEntries.Count > 0)
                {
                    entries = jsonPathEntries;
                }

                Dictionary<string, IReadOnlyList<string>> fieldErrors = entries
                    .SelectMany(e => ToFieldErrors(e.Key, e.Error))
                    .GroupBy(e => e.Field, StringComparer.Ordinal)
                    .ToDictionary(group => group.Key, IReadOnlyList<string> (group) => [.. group.Select(e => e.Message).Distinct()], StringComparer.Ordinal);

                IOperationResult result = OperationResult.Failure(fieldErrors);
                return result.ToActionResult(context.HttpContext);
            });

        return services;
    }

    [GeneratedRegex(@"missing required properties including: (?<props>.+)\.$")]
    private static partial Regex MissingPropertiesRegex();

    // Raw System.Text.Json messages leak .NET type names/internals — collapse to plain messages.
    private static IEnumerable<(string Field, string Message)> ToFieldErrors(string key, ModelError error)
    {
        if (key == "$")
        {
            if (MissingPropertiesRegex().Match(error.ErrorMessage) is { Success: true } match)
            {
                return match.Groups["props"].Value
                    .Replace("'", "")
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(field => (ToFieldName(field), "This field is required."));
            }

            return [(string.Empty, "The request body is missing or has an invalid format.")];
        }

        if (key.StartsWith("$.", StringComparison.Ordinal))
        {
            return [(ToFieldName(key[2..]), "The value is missing or has an invalid format.")];
        }

        return [(ToFieldName(key), error.ErrorMessage)];
    }

    private static string ToFieldName(string path)
        => string.Join('.', path.Split('.').Select(segment => segment.Length == 0 ? segment : char.ToUpperInvariant(segment[0]) + segment[1..]));
}
