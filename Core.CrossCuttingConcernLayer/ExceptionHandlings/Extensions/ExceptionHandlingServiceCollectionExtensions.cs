using Core.CrossCuttingConcernLayer.ExceptionHandlings.Constants;
using Core.CrossCuttingConcernLayer.ExceptionHandlings.Exceptions;
using Core.CrossCuttingConcernLayer.ExceptionHandlings.Handlers;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ResultHandler.AspNetCore.Extensions;
using ResultHandler.Facade;

namespace Core.CrossCuttingConcernLayer.ExceptionHandlings.Extensions;

public static class ExceptionHandlingServiceCollectionExtensions
{
    private const string ConcurrencyMessage = "This record was modified by someone else. Please reload and try again.";
    private const string ConflictMessage = "This operation conflicts with existing data. Please check your input and try again.";

    /// <summary>
    /// Registers the exception handling used by <see cref="ExceptionMiddlewareExtensions.UseConfigureCustomExceptionMiddleware"/>: every unhandled
    /// exception is logged with the registered <see cref="Loggings.Serilogs.Services.BaseLoggerService"/>, then answered by ResultHandler as Problem
    /// Details. Business, query and database exceptions map to 400/409; anything else is a 500 whose message reaches the client only in Development.
    /// Every answer carries an error code: <see cref="ExceptionErrorCodes"/> for the mapped exceptions, ResultHandler's built-in codes for the rest.
    /// </summary>
    public static IServiceCollection AddConfigureCustomExceptionHandling(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IExceptionHandler, ExceptionLoggingHandler>());
        return services.AddResultExceptionHandler(options =>
        {
            options.IncludeErrorCodes = true;
            _ = options
                .Map<BusinessRuleException>(exception => Result.Conflict(exception.Message).WithCode(ExceptionErrorCodes.BusinessRuleViolated))
                .Map<DynamicQueryException>(exception => Result.BadRequest(exception.Message).WithCode(ExceptionErrorCodes.InvalidQuery))
                .Map<DbUpdateConcurrencyException>(_ => Result.Conflict(ConcurrencyMessage).WithCode(ExceptionErrorCodes.ConcurrencyConflict))
                .Map<DbUpdateException>(_ => Result.Conflict(ConflictMessage).WithCode(ExceptionErrorCodes.DataConflict));
        });
    }
}
