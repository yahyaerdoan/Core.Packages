using System.Security.Claims;
using Core.SecurityLayer.Constants;

namespace Core.SecurityLayer.Extensions;

public static class ClaimPrincipalExtension
{
    public static List<string>? Claims(this ClaimsPrincipal claimsPrincipal, string claimType)
    {
        List<string>? result = claimsPrincipal?.FindAll(claimType)?.Select(x => x.Value).ToList();
        return result;
    }

    public static List<string>? ClaimRoles(this ClaimsPrincipal claimsPrincipal)
    {
        return claimsPrincipal?.Claims(ClaimTypes.Role);
    }

    public static List<string>? ClaimPermissions(this ClaimsPrincipal claimsPrincipal)
    {
        return claimsPrincipal?.Claims(PermissionClaimTypes.Type);
    }

    // TKey-generic since IdentityUser<TKey> may use Guid, int, or string.
    public static TKey GetUserId<TKey>(this ClaimsPrincipal claimsPrincipal) where TKey : IParsable<TKey>
    {
        return TKey.Parse(claimsPrincipal?.Claims(ClaimTypes.NameIdentifier)?.FirstOrDefault()
            ?? throw new InvalidOperationException("Principal has no NameIdentifier claim."), null);
    }
}
