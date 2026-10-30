using System.Text.Json;
using Core.CrossCuttingConcernLayer.Loggings.Parameters;
using Core.CrossCuttingConcernLayer.Loggings.Serilogs.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using ResultHandler.AspNetCore.Exceptions;
using ResultHandler.Mapping;

namespace Core.CrossCuttingConcernLayer.ExceptionHandlings.Handlers;

/// <summary>Writes every unhandled exception to <see cref="BaseLoggerService"/> (as an error when it is answered with a 5xx, as a warning otherwise), then lets the next handler write the response.</summary>
internal sealed class ExceptionLoggingHandler(BaseLoggerService baseLoggerService, IOptions<ResultExceptionHandlerOptions> options) : IExceptionHandler
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

        string json = JsonSerializer.Serialize(logDetail);
        if ((int)options.Value.Resolve(exception, httpContext).Status.ToHttpStatusCode() >= StatusCodes.Status500InternalServerError)
        {
            baseLoggerService.Error(json);
        }
        else
        {
            baseLoggerService.Warn(json);
        }

        return ValueTask.FromResult(false);
    }
}
