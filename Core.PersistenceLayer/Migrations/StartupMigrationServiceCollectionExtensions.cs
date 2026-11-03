using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Core.PersistenceLayer.Migrations;

public static class StartupMigrationServiceCollectionExtensions
{
    /// <summary>Has <see cref="StartupMigrationExtensions.MigrateDatabasesAsync"/> bring <typeparamref name="TContext"/>'s database up to date,
    /// so a deploy never runs code against a schema it is ahead of. Register it next to the context; registering it twice is harmless.</summary>
    public static IServiceCollection AddStartupMigration<TContext>(this IServiceCollection services)
        where TContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(services);

        return services.AddSingleton(new StartupMigrationTarget(typeof(TContext)));
    }
}
