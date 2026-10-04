using Core.PersistenceLayer.Repositories.Auditing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Core.ApplicationLayer.Auditing;

public static class AuditingServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="AuditSaveChangesInterceptor"/> with the request user as the auditor; add it to each DbContext with
    /// AddInterceptors(serviceProvider.GetRequiredService&lt;AuditSaveChangesInterceptor&gt;()). Register your own IAuditUserProvider first to replace the user source.
    /// </summary>
    public static IServiceCollection AddAuditing(this IServiceCollection services)
    {
        _ = services.AddHttpContextAccessor();
        services.TryAddSingleton(TimeProvider.System);
        services.TryAddScoped<IAuditUserProvider, HttpContextAuditUserProvider>();
        services.TryAddScoped<AuditSaveChangesInterceptor>();
        return services;
    }
}
