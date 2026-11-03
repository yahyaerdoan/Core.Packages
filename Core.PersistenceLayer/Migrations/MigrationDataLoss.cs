using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Core.PersistenceLayer.Migrations;

/// <summary>Finds the steps of a migration that can destroy existing data: dropping a table, column or schema, deleting rows, narrowing a
/// column (a shorter length, a different type, or nullable to required), or raw SQL that drops, deletes or truncates.</summary>
public static partial class MigrationDataLoss
{
    /// <summary>A short description of each destructive step; empty when the migration only adds or renames.</summary>
    public static IReadOnlyList<string> Describe(IEnumerable<MigrationOperation> operations)
    {
        ArgumentNullException.ThrowIfNull(operations);

        return [.. operations.Select(Describe).OfType<string>()];
    }

    private static string? Describe(MigrationOperation operation) => operation switch
    {
        DropTableOperation drop => $"drops table {Name(drop.Schema, drop.Name)}",
        DropColumnOperation drop => $"drops column {Name(drop.Schema, drop.Table)}.{drop.Name}",
        DropSchemaOperation drop => $"drops schema {drop.Name}",
        DeleteDataOperation delete => $"deletes rows from {Name(delete.Schema, delete.Table)}",
        AlterColumnOperation alter when Narrows(alter) => $"narrows column {Name(alter.Schema, alter.Table)}.{alter.Name}",
        SqlOperation sql when DestructiveSql().IsMatch(sql.Sql) => "runs SQL that drops, deletes or truncates",
        _ => null,
    };

    private static bool Narrows(AlterColumnOperation alter)
    {
        var old = alter.OldColumn;

        return alter.ClrType != old.ClrType
            || (alter.MaxLength ?? int.MaxValue) < (old.MaxLength ?? int.MaxValue)
            || (alter.Precision ?? int.MaxValue) < (old.Precision ?? int.MaxValue)
            || (alter.Scale ?? int.MaxValue) < (old.Scale ?? int.MaxValue)
            || (old.IsNullable && !alter.IsNullable);
    }

    private static string Name(string? schema, string name) => schema is null ? name : $"{schema}.{name}";

    [GeneratedRegex(@"\b(DROP|DELETE|TRUNCATE)\b", RegexOptions.IgnoreCase)]
    private static partial Regex DestructiveSql();
}
