using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Core.CrossCuttingConcernLayer.ExceptionHandlings.Extensions;

public static class ExceptionMiddlewareExtensions
{
    /// <summary>Adds ASP.NET Core's exception handler middleware, answering with the handlers <see cref="ExceptionHandlingServiceCollectionExtensions.AddConfigureCustomExceptionHandling"/> registers. Call it early so it covers the rest of the pipeline.</summary>
    public static void UseConfigureCustomExceptionMiddleware(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        if (!app.ApplicationServices.GetServices<IExceptionHandler>().Any())
        {
            throw new InvalidOperationException($"No exception handler is registered. Call services.{nameof(ExceptionHandlingServiceCollectionExtensions.AddConfigureCustomExceptionHandling)}() before {nameof(UseConfigureCustomExceptionMiddleware)}().");
        }

        _ = app.UseExceptionHandler();
    }
}
