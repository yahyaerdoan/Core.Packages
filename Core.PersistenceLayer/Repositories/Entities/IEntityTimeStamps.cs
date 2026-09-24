namespace Core.PersistenceLayer.Repositories.Entities;

public interface IEntityTimeStamps
{
    DateTimeOffset CreatedDate { get; set; }
    DateTimeOffset? UpdatedDate { get; set; }
    DateTimeOffset? DeletedDate { get; set; }
}
