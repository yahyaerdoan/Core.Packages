namespace Core.PersistenceLayer.MultiTenancy;

/// <summary>A save would have written another tenant's data, or tenant-owned data with no tenant at all.</summary>
public sealed class TenantIsolationException(string message) : InvalidOperationException(message);
