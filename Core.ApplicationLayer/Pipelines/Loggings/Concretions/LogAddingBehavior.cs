using System.Reflection;
using System.Text.Json;

using Core.ApplicationLayer.Pipelines.Loggings.Abstractions;
using Core.CrossCuttingConcernLayer.Loggings.Parameters;
using Core.CrossCuttingConcernLayer.Loggings.Serilogs.Services;

using MediatR;

using Microsoft.AspNetCore.Http;

namespace Core.ApplicationLayer.Pipelines.Loggings.Concretions;

public class LogAddingBehavior<TRequest, TResponse>(IHttpContextAccessor httpContextAccessor, BaseLoggerService baseLoggerService)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>, ILogAddRequest
{
    private const string RedactedValue = "***REDACTED***";

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        List<LogParameter> logParameters = [new() { Type = request.GetType().Name, Value = Redact(request) }];

        LogDetail logDetail = new() { MethodName = next.Method.Name, Parameters = logParameters, User = httpContextAccessor.HttpContext?.User.Identity?.Name ?? "?" };
        baseLoggerService.Info(JsonSerializer.Serialize(logDetail));
        return await next(cancellationToken);
    }

    private static object Redact(TRequest request)
    {
        var properties = typeof(TRequest).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var hasSensitiveData = properties.Any(p => p.GetCustomAttribute<SensitiveDataAttribute>() is not null);
        if (!hasSensitiveData)
        {
            return request;
        }

        return properties.ToDictionary(
            p => p.Name,
            p => p.GetCustomAttribute<SensitiveDataAttribute>() is not null ? RedactedValue : p.GetValue(request));
    }
}
