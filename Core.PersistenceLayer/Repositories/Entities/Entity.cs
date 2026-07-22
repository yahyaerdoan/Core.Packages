namespace Core.PersistenceLayer.Repositories.Entities;

public class Entity<TId> : IEntityTimeStamps
{
    public TId Id { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset? UpdatedDate { get; set; }
    public DateTimeOffset? DeletedDate { get; set; }
    public Entity()
    {
        Id = default;
    }
    public Entity(TId id)
    {
        Id = id;
    }
}
