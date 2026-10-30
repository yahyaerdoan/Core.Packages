using System.Text.Json;
using Core.CrossCuttingConcernLayer.Loggings.Parameters;
using Core.CrossCuttingConcernLayer.Loggings.Serilogs.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace Core.CrossCuttingConcernLayer.ExceptionHandlings.Handlers;

/// <summary>Writes every unhandled exception to <see cref="BaseLoggerService"/>, then lets the next handler write the response.</summary>
internal sealed class ExceptionLoggingHandler(BaseLoggerService baseLoggerService) : IExceptionHandler
{
    public ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        ExceptionLogDetail logDetail = new()
        {
            ExceptionMessage = exception.Message,
            MethodName = httpContext.Features.Get<IExceptionHandlerFeature>()?.Endpoint?.DisplayName ?? httpContext.Request.Path.Value ?? "?",
            Parameters = [new LogParameter { Type = exception.GetType().Name, Value = exception.ToString() }],
            User = httpContext.User.Identity?.Name ?? "?",
        };

        baseLoggerService.Error(JsonSerializer.Serialize(logDetail));
        return ValueTask.FromResult(false);
    }
}
