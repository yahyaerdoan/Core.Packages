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
    // Model-binding failures bypass ResultHandler and MediatR entirely (ExceptionMiddleware can't
    // catch them either — invalid ModelState never throws) — route them through ResultHandler here.
    public static IServiceCollection AddConfigureCustomModelValidation(this IServiceCollection services)
    {
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var entries = context.ModelState
                    .Where(entry => entry.Value?.Errors.Count > 0)
                    .SelectMany(entry => entry.Value!.Errors.Select(error => (entry.Key, Error: error)))
                    .ToList();

                // Prefer JSON-path-keyed entries ("$.modelId") over the vague parameter-name one.
                var jsonPathEntries = entries.Where(e => e.Key.StartsWith('$')).ToList();
                if (jsonPathEntries.Count > 0)
                {
                    entries = jsonPathEntries;
                }

                string[] errors = [.. entries.Select(e => FormatFieldError(e.Key, e.Error))];

                IOperationResult result = OperationResult.Failure(errors);
                return result.ToActionResult(context.HttpContext);
            };
        });

        return services;
    }

    [GeneratedRegex(@"missing required properties including: (?<props>.+)\.$")]
    private static partial Regex MissingPropertiesRegex();

    // Raw System.Text.Json messages leak .NET type names/internals — collapse to plain messages.
    private static string FormatFieldError(string key, ModelError error)
    {
        if (key == "$" && MissingPropertiesRegex().Match(error.ErrorMessage) is { Success: true } match)
        {
            var fields = match.Groups["props"].Value.Replace("'", "");
            return $"{fields}: This field is required.";
        }

        if (key.StartsWith("$.", StringComparison.Ordinal))
        {
            var field = key[2..];
            return $"{field}: The value is missing or has an invalid format.";
        }

        return $"{key}: {error.ErrorMessage}";
    }
}
