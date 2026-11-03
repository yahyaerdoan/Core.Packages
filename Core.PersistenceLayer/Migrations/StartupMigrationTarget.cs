namespace Core.PersistenceLayer.Migrations;

/// <summary>A DbContext whose pending migrations <see cref="StartupMigrationExtensions.MigrateDatabasesAsync"/> applies.</summary>
public sealed record StartupMigrationTarget(Type ContextType);
