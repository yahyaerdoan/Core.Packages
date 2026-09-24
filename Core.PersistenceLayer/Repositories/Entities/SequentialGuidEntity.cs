namespace Core.PersistenceLayer.Repositories.Entities;

/// <summary>Guid-keyed entity that self-assigns a UUIDv7 (time-ordered) id - avoids clustered-index
/// fragmentation from random inserts. Purely additive, opt-in - plain Entity of any key type is
/// unaffected. Same technique as Core.SecurityLayer's SequentialGuidIdentityUser/Role.</summary>
public abstract class SequentialGuidEntity : Entity<Guid>
{
    protected SequentialGuidEntity()
    {
        Id = Guid.CreateVersion7();
    }
}
