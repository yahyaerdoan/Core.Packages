using System.Text.Json;
using Core.CrossCuttingConcernLayer.ExceptionHandlings.Exceptions;
using Core.CrossCuttingConcernLayer.Loggings.Parameters;
using Core.CrossCuttingConcernLayer.Loggings.Serilogs.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using ResultHandler.AspNetCore.Extensions;
using ResultHandler.Core.Abstractions;
using ResultHandler.Facade;

namespace Core.CrossCuttingConcernLayer.ExceptionHandlings.Middlewares;

public class ExceptionMiddleware(RequestDelegate next, IHttpContextAccessor httpContextAccessor, BaseLoggerService baseLoggerService, IHostEnvironment hostEnvironment)
{
    private const string GenericInternalServerErrorMessage = "An unexpected error occurred. Please try again later.";

    public async Task Invoke(HttpContext httpContext)
    {
        try
        {
            await next(httpContext);
        }
        catch (OperationCanceledException) when (httpContext.RequestAborted.IsCancellationRequested)
        {
            // Client disconnected or cancelled the request; nothing to log or respond to.
        }
        catch (Exception exception)
        {
            await LogException(httpContext, exception);
            await HandleExceptionAsync(httpContext, exception);
        }
    }

    private Task LogException(HttpContext httpContext, Exception exception)
    {
        List<LogParameter> logParameters = [new LogParameter { Type = exception.GetType().Name, Value = exception.ToString() }];

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

    private Task HandleExceptionAsync(HttpContext httpContext, Exception exception)
    {
        httpContext.Response.ContentType = "application/json";

        IOperationResult result = exception switch
        {
            BadHttpRequestException badHttpRequestException => Result.BadRequest(badHttpRequestException.Message),
            JsonException jsonException => Result.BadRequest(jsonException.Message),
            BusinessRuleException businessRuleException => Result.Conflict(businessRuleException.Message),
            DbUpdateConcurrencyException => Result.Conflict("This record was modified by someone else. Please reload and try again."),
            DbUpdateException => Result.Conflict("This operation conflicts with existing data. Please check your input and try again."),
            // Don't echo raw exception text in production - it can leak internals. Full detail is
            // already captured server-side via LogException above.
            _ => Result.InternalServerError(hostEnvironment.IsDevelopment() ? exception.Message : GenericInternalServerErrorMessage)
        };

        ProblemDetails problem = result.ToProblemDetails(httpContext);
        httpContext.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        return httpContext.Response.WriteAsJsonAsync(problem);
    }
}
