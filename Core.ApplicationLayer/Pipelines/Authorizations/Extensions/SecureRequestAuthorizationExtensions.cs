using System.Security.Claims;
using Core.ApplicationLayer.Pipelines.Authorizations.Abstractions;
using Core.SecurityLayer.Constants;
using Core.SecurityLayer.Extensions;

namespace Core.ApplicationLayer.Pipelines.Authorizations.Extensions;

public static class SecureRequestAuthorizationExtensions
{
    /// <summary>True when <paramref name="user"/> passes <see cref="Concretions.AuthorizationAddingBehavior{TRequest, TResponse}"/> for <paramref name="request"/>.</summary>
    public static bool IsAuthorizedFor(this ClaimsPrincipal? user, ISecureAddRequest request)
    {
        if (user?.Identity?.IsAuthenticated is not true)
        {
            return false;
        }

        if (request.Roles.Length == 0)
        {
            return true;
        }

        List<string> roles = user.ClaimRoles() ?? [];
        List<string> permissions = user.ClaimPermissions() ?? [];

        return permissions.Contains(PermissionClaimTypes.FullAccess)
            || (request.AllowTenantBypass && permissions.Contains(PermissionClaimTypes.TenantFullAccess))
            || request.Roles.Any(role => roles.Contains(role) || permissions.Contains(role));
    }
}
