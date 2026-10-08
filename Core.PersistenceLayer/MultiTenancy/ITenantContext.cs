namespace Core.PersistenceLayer.MultiTenancy;

/// <summary>The tenant the current request or job acts for, as the data layer sees it. Null means unknown: tenant-scoped reads then return
/// nothing and writes throw, instead of falling back to some default tenant.</summary>
public interface ITenantContext
{
    Guid? TenantId { get; }
}
