namespace Core.PersistenceLayer.Repositories.Entities;

public interface IEntityAuditor : IEntityTimeStamps
{
    string? CreatedBy { get; set; }
    string? UpdatedBy { get; set; }
    string? DeletedBy { get; set; }
}
