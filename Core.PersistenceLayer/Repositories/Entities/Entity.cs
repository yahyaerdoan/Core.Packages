namespace Core.PersistenceLayer.Repositories.Entities;

public class Entity<TId> : IEntityAuditor
{
    public TId Id { get; set; } = default!;
    public DateTimeOffset CreatedDate { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedDate { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTimeOffset? DeletedDate { get; set; }
    public string? DeletedBy { get; set; }
    public byte[]? RowVersion { get; set; }
    public Entity()
    {
    }
    public Entity(TId id)
    {
        Id = id;
    }
}
