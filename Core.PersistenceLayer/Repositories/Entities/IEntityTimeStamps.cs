namespace Core.PersistenceLayer.Repositories.Entities;

public interface IEntityTimeStamps
{
    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset? UpdatedDate { get; set; }
    public DateTimeOffset? DeletedDate { get; set; }
}
