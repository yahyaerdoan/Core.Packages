namespace Core.PersistenceLayer.Repositories.Entities;

/// <summary>Named query filter keys - lets withDeleted ignore only soft-delete, not every filter (e.g. tenant isolation) an entity might also carry.</summary>
public static class QueryFilterNames
{
    public const string SoftDelete = "SoftDelete";
}
