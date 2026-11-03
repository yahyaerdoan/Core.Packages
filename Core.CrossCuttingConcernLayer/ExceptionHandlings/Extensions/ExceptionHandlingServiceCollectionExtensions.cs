using Core.CrossCuttingConcernLayer.ExceptionHandlings.Constants;
using Core.CrossCuttingConcernLayer.ExceptionHandlings.Exceptions;
using Core.CrossCuttingConcernLayer.ExceptionHandlings.Handlers;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ResultHandler.AspNetCore.Exceptions;
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
    /// Details. Business, query and concurrency exceptions map to 400/409, and so does a database write the data refused (a duplicate value, a
    /// missing or still-referenced row); any other database failure, such as a schema the code is ahead of, is a 500 like anything else, whose
    /// message reaches the client only in Development and which is logged as an error.
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
                .Map<DbUpdateException>(exception => ConstraintViolations.IsMatch(exception)
                    ? Result.Conflict(ConflictMessage).WithCode(ExceptionErrorCodes.DataConflict)
                    : (options.ExposeExceptionMessages ? Result.InternalServerError(exception.Message) : Result.InternalServerError()).WithCode(ResultErrorCodes.ServerError));
        });
    }
}
