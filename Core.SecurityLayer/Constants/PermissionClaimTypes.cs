namespace Core.SecurityLayer.Constants;

public static class PermissionClaimTypes
{
    public const string Type = "permission";

    // Bypasses every ISecureAddRequest check - not tied to any specific role name.
    public const string FullAccess = "system.full-access";
}
