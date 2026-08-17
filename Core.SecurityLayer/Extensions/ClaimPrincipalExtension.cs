using Core.SecurityLayer.Constants;
using System.Security.Claims;

namespace Core.SecurityLayer.Extensions;

public static class ClaimPrincipalExtension
{
    public static List<string>? Claims(this ClaimsPrincipal claimsPrincipal, string claimType)
    {
        var result = claimsPrincipal?.FindAll(claimType)?.Select(x => x.Value).ToList();
        return result;
    }

    public static List<string>? ClaimRoles(this ClaimsPrincipal claimsPrincipal) =>
        claimsPrincipal?.Claims(ClaimTypes.Role);

    public static List<string>? ClaimPermissions(this ClaimsPrincipal claimsPrincipal) =>
        claimsPrincipal?.Claims(PermissionClaimTypes.Type);

    // TKey-generic since IdentityUser<TKey> may use Guid, int, or string.
    public static TKey GetUserId<TKey>(this ClaimsPrincipal claimsPrincipal) where TKey : IParsable<TKey> =>
        TKey.Parse(claimsPrincipal?.Claims(ClaimTypes.NameIdentifier)?.FirstOrDefault()
            ?? throw new InvalidOperationException("Principal has no NameIdentifier claim."), null);
}
