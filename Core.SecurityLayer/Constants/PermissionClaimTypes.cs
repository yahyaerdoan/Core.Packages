namespace Core.SecurityLayer.Constants;

public static class PermissionClaimTypes
{
    public const string Type = "permission";

    // Global bypass - every ISecureAddRequest check, no exceptions. For cross-tenant/vendor-level admins.
    public const string FullAccess = "system.full-access";

    // Scoped bypass - same as FullAccess, but honors AllowTenantBypass, so requests can opt out. Grant to
    // tenant-scoped admins instead of FullAccess.
    public const string TenantFullAccess = "tenant.full-access";
}
