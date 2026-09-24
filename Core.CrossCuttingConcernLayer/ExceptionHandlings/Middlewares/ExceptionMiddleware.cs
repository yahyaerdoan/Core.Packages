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
    private const string ProblemJsonContentType = "application/problem+json";
    private const string GenericInternalServerErrorMessage = "An unexpected error occurred. Please try again later.";
    private const string GenericBadRequestMessage = "The request could not be read. Please check the request format and try again.";
    private const string GenericInvalidJsonMessage = "The request body is not valid JSON.";

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

            if (httpContext.Response.HasStarted)
            {
                throw;
            }

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
        IOperationResult result = exception switch
        {
            BadHttpRequestException badHttpRequestException => Result.BadRequest(ClientSafe(badHttpRequestException.Message, GenericBadRequestMessage)),
            JsonException jsonException => Result.BadRequest(ClientSafe(jsonException.Message, GenericInvalidJsonMessage)),
            BusinessRuleException businessRuleException => Result.Conflict(businessRuleException.Message),
            DbUpdateConcurrencyException => Result.Conflict("This record was modified by someone else. Please reload and try again."),
            DbUpdateException => Result.Conflict("This operation conflicts with existing data. Please check your input and try again."),
            _ => Result.InternalServerError(ClientSafe(exception.Message, GenericInternalServerErrorMessage))
        };

        ProblemDetails problem = result.ToProblemDetails(httpContext);
        httpContext.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        return httpContext.Response.WriteAsJsonAsync(problem, options: null, contentType: ProblemJsonContentType);
    }

    private string ClientSafe(string developmentMessage, string productionMessage)
        => hostEnvironment.IsDevelopment() ? developmentMessage : productionMessage;
}
