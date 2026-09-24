namespace Core.PersistenceLayer.Repositories.Entities;

/// <summary>Opt-in Guid entity - self-assigns a sequential (UUIDv7) id to avoid index fragmentation.</summary>
public abstract class SequentialGuidEntity : Entity<Guid>
{
    protected SequentialGuidEntity()
    {
        Id = Guid.CreateVersion7();
    }
}
