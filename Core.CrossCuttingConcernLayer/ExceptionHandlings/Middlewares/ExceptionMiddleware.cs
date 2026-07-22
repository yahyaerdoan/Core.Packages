using System.Text.Json;

using Core.CrossCuttingConcernLayer.ExceptionHandlings.Handlers;
using Core.CrossCuttingConcernLayer.Loggings.Parameters;
using Core.CrossCuttingConcernLayer.Loggings.Serilogs.Services;

using Microsoft.AspNetCore.Http;

namespace Core.CrossCuttingConcernLayer.ExceptionHandlings.Middlewares;

public class ExceptionMiddleware(RequestDelegate next, IHttpContextAccessor httpContextAccessor, BaseLoggerService baseLoggerService)
{
    private readonly HttpExceptionHandler _httpExceptionHandler = new();

    public async Task Invoke(HttpContext httpContext)
    {
        try
        {
            await next(httpContext);
        }
        catch (Exception exception)
        {
            await LogException(httpContext, exception);
            await HandleExceptionAsync(httpContext.Response, exception);
        }
    }

    private Task LogException(HttpContext httpContext, Exception exception)
    {
        List<LogParameter> logParameters = [new LogParameter { Type = httpContext.GetType().Name, Value = exception.ToString() }];

        ExceptionLogDetail logDetail = new()
        {
            ExceptionMessage = exception.Message,
            MethodName = next.Method.Name,
            Parameters = logParameters,
            User = httpContextAccessor.HttpContext?.User.Identity?.Name ?? "?"
        };

        baseLoggerService.Error(JsonSerializer.Serialize(logDetail));

        return Task.CompletedTask;
    }

    private Task HandleExceptionAsync(HttpResponse response, Exception exception)
    {
        response.ContentType = "application/json";
        _httpExceptionHandler.Response = response;
        return _httpExceptionHandler.HandleExceptionsAsync(exception);
    }
}
