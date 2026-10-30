using System.Text.Json;
using Core.CrossCuttingConcernLayer.ExceptionHandlings.Exceptions;
using Core.CrossCuttingConcernLayer.ExceptionHandlings.Handlers;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using ResultHandler.AspNetCore.Extensions;
using ResultHandler.Facade;

namespace Core.CrossCuttingConcernLayer.ExceptionHandlings.Extensions;

public static class ExceptionHandlingServiceCollectionExtensions
{
    private const string InvalidJsonMessage = "The request body is not valid JSON.";
    private const string ConcurrencyMessage = "This record was modified by someone else. Please reload and try again.";
    private const string ConflictMessage = "This operation conflicts with existing data. Please check your input and try again.";

    /// <summary>
    /// Registers the exception handling used by <see cref="ExceptionMiddlewareExtensions.UseConfigureCustomExceptionMiddleware"/>: every unhandled
    /// exception is logged with the registered <see cref="Loggings.Serilogs.Services.BaseLoggerService"/>, then answered by ResultHandler as Problem
    /// Details. Business, query and database exceptions map to 400/409; anything else is a 500 whose message reaches the client only in Development.
    /// </summary>
    public static IServiceCollection AddConfigureCustomExceptionHandling(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IExceptionHandler, ExceptionLoggingHandler>());
        return services.AddResultExceptionHandler(options => options
            .Map<BusinessRuleException>(exception => Result.Conflict(exception.Message))
            .Map<DynamicQueryException>(exception => Result.BadRequest(exception.Message))
            .Map<JsonException>((exception, httpContext) => Result.BadRequest(IsDevelopment(httpContext) ? exception.Message : InvalidJsonMessage))
            .Map<DbUpdateConcurrencyException>(_ => Result.Conflict(ConcurrencyMessage))
            .Map<DbUpdateException>(_ => Result.Conflict(ConflictMessage)));
    }

    private static bool IsDevelopment(HttpContext httpContext)
        => httpContext.RequestServices.GetRequiredService<IHostEnvironment>().IsDevelopment();
}
