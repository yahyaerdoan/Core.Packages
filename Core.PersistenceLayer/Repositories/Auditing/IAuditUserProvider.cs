namespace Core.PersistenceLayer.Repositories.Auditing;

/// <summary>Who is saving; stamped into CreatedBy, UpdatedBy and DeletedBy. Null for anonymous or background work.</summary>
public interface IAuditUserProvider
{
    string? UserId { get; }
}
