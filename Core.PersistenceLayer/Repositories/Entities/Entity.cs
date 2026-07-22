namespace Core.PersistenceLayer.Repositories.Entities;

public class Entity<TId> : IEntityTimeStamps
{
    public TId Id { get; set; } = default!;
    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset? UpdatedDate { get; set; }
    public DateTimeOffset? DeletedDate { get; set; }
    public Entity()
    {
    }
    public Entity(TId id)
    {
        Id = id;
    }
}
