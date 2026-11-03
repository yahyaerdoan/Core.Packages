using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Core.PersistenceLayer.Migrations;

public static partial class StartupMigrationExtensions
{
    /// <summary>
    /// Applies the pending migrations of every context registered with <see cref="StartupMigrationServiceCollectionExtensions.AddStartupMigration{TContext}"/>,
    /// one after another; call it once at startup, before the app serves requests. A migration that would destroy data (see <see cref="MigrationDataLoss"/>)
    /// stops startup unless <paramref name="allowDataLoss"/> is set for that deploy, so dropping or narrowing is always a decision someone made; a database with
    /// no migrations applied yet has nothing to lose and is created in full. Concurrent instances are safe: EF Core takes a lock while it migrates.
    /// </summary>
    /// <exception cref="InvalidOperationException">A pending migration would destroy data and <paramref name="allowDataLoss"/> is false.</exception>
    public static async Task MigrateDatabasesAsync(this IServiceProvider services, bool allowDataLoss = false, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(services);

        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(StartupMigrationExtensions));
        var targets = services.GetServices<StartupMigrationTarget>().Select(target => target.ContextType).Distinct();

        foreach (var contextType in targets)
        {
            await using var scope = services.CreateAsyncScope();
            var context = (DbContext)scope.ServiceProvider.GetRequiredService(contextType);

            var pending = (await context.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
            if (pending.Count == 0)
            {
                continue;
            }

            var hasData = (await context.Database.GetAppliedMigrationsAsync(cancellationToken)).Any();
            var losses = hasData ? DataLossOf(context, pending) : [];
            if (losses.Count > 0 && !allowDataLoss)
            {
                throw new InvalidOperationException(
                    $"{contextType.Name} has pending migrations that would destroy data, so they were not applied: {string.Join("; ", losses)}. " +
                    "Back the data up or move it, then deploy once with data loss allowed.");
            }

            LogApplying(logger, pending.Count, contextType.Name, pending);
            await context.Database.MigrateAsync(cancellationToken);
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Applying {Count} migration(s) to {Context}: {Migrations}")]
    private static partial void LogApplying(ILogger logger, int count, string context, IReadOnlyList<string> migrations);

    private static List<string> DataLossOf(DbContext context, IEnumerable<string> pending)
    {
        var migrations = context.GetService<IMigrationsAssembly>();
        var provider = context.Database.ProviderName ?? throw new InvalidOperationException($"{context.GetType().Name} has no database provider.");

        return [.. pending.SelectMany(id => MigrationDataLoss.Describe(migrations.CreateMigration(migrations.Migrations[id], provider).UpOperations).Select(loss => $"{id} {loss}"))];
    }
}
