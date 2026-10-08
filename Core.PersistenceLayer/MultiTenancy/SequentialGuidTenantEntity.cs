namespace Core.PersistenceLayer.MultiTenancy;

/// <summary>Guid-keyed TenantEntity that self-assigns a UUIDv7 (time-ordered) id, like SequentialGuidEntity.</summary>
public abstract class SequentialGuidTenantEntity : TenantEntity<Guid>
{
    protected SequentialGuidTenantEntity()
    {
        Id = Guid.CreateVersion7();
    }

    /// <summary>Same id the constructor would assign, for when it's needed before the entity exists (e.g. an idempotency key).</summary>
    public static Guid NewId() => Guid.CreateVersion7();
}
