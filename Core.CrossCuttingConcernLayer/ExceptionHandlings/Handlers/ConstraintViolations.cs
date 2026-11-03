using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace Core.CrossCuttingConcernLayer.ExceptionHandlings.Handlers;

/// <summary>Tells a write the data refused (a duplicate value, a missing or still-referenced row, a failed check) from any other database
/// failure such as a missing column or a lost connection, on SQL Server, PostgreSQL and SQLite, without referencing their client libraries.</summary>
internal static class ConstraintViolations
{
    private const int SqliteConstraint = 19;

    private static readonly HashSet<int> SqlServerConstraintNumbers = [2601, 2627, 547];

    public static bool IsMatch(DbUpdateException exception) => exception.InnerException is DbException database
        && (database.SqlState?.StartsWith("23", StringComparison.Ordinal) == true
            || (ErrorNumber(database, "Number") is { } number && SqlServerConstraintNumbers.Contains(number))
            || ErrorNumber(database, "SqliteErrorCode") == SqliteConstraint);

    private static int? ErrorNumber(DbException exception, string property) => exception.GetType().GetProperty(property)?.GetValue(exception) as int?;
}
