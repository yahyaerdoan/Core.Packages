using System.Reflection;
using System.Text.Json;
using Core.ApplicationLayer.Pipelines.Loggings.Abstractions;
using Core.CrossCuttingConcernLayer.Loggings.Parameters;
using Core.CrossCuttingConcernLayer.Loggings.Serilogs.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using ResultHandler.Core.Abstractions;

namespace Core.ApplicationLayer.Pipelines.Loggings.Concretions;

/// <summary>Logs the outcome of a request after it runs, since non-throwing IOperationResult failures
/// (e.g. Result.BadRequest) would otherwise leave no trace in the log. Failures log at Warn, successes
/// at Info. Complements LogAddingBehavior, which logs the request itself before it runs.</summary>
public class LogResultAddingBehavior<TRequest, TResponse>(IHttpContextAccessor httpContextAccessor, BaseLoggerService baseLoggerService)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, ILogResultRequest
    where TResponse : IOperationResult
{
    private const string RedactedValue = "***REDACTED***";

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        TResponse response = await next(cancellationToken);

        List<LogParameter> logParameters = [new() { Type = request.GetType().Name, Value = Redact(request) }];
        LogDetail logDetail = new()
        {
            MethodName = next.Method.Name,
            Parameters = logParameters,
            User = httpContextAccessor.HttpContext?.User.Identity?.Name ?? "?",
            Result = new
            {
                response.IsSuccessful,
                Status = response.Status.ToString(),
                response.Title,
                response.Detail,
                response.Errors,
            },
        };

        string json = JsonSerializer.Serialize(logDetail);
        if (response.IsSuccessful)
        {
            baseLoggerService.Info(json);
        }
        else
        {
            baseLoggerService.Warn(json);
        }

        return response;
    }

    private static readonly PropertyInfo[] Properties = typeof(TRequest).GetProperties(BindingFlags.Public | BindingFlags.Instance);

    private static readonly PropertyInfo[] SensitiveProperties =
        Properties.Where(p => p.GetCustomAttribute<SensitiveDataAttribute>() is not null).ToArray();

    private static object Redact(TRequest request)
    {
        return SensitiveProperties.Length == 0
            ? request
            : Properties.ToDictionary(p => p.Name, p => SensitiveProperties.Contains(p) ? RedactedValue : p.GetValue(request));
    }
}
