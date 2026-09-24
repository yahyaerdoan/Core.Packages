namespace Core.ApplicationLayer.Pipelines.Authorizations.Abstractions;

public interface ISecureAddRequest
{
    string[] Roles { get; }

    /// <summary>Set false to block TenantFullAccess bypass and require a literal Roles match - for
    /// platform/cross-tenant operations a tenant admin must never reach. FullAccess still bypasses.</summary>
    bool AllowTenantBypass => true;
}
