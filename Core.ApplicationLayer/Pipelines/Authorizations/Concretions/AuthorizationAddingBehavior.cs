using Core.ApplicationLayer.Extensions;
using Core.ApplicationLayer.Pipelines.Authorizations.Abstractions;
using Core.ApplicationLayer.Pipelines.Authorizations.Constants;
using Core.SecurityLayer.Constants;
using Core.SecurityLayer.Extensions;
using MediatR;
using Microsoft.AspNetCore.Http;
using ResultHandler.Core.Abstractions;
using ResultHandler.Functional;

namespace Core.ApplicationLayer.Pipelines.Authorizations.Concretions;

public class AuthorizationAddingBehavior<TRequest, TResponse>(IHttpContextAccessor httpContextAccessor)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, ISecureAddRequest
    where TResponse : IOperationResult, IResultFailureFactory<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated is not true)
        {
            return ResultFailureFactory.Unauthorized<TResponse>("You are not authenticated.").WithErrorCode(AuthorizationErrorCodes.Unauthenticated);
        }

        if (request.Roles.Length == 0)
        {
            return await next(cancellationToken);
        }

        List<string> userRoleClaims = httpContextAccessor.HttpContext.User.ClaimRoles() ?? [];
        List<string> userPermissionClaims = httpContextAccessor.HttpContext.User.ClaimPermissions() ?? [];

        bool isMatched = userPermissionClaims.Contains(PermissionClaimTypes.FullAccess)
            || (request.AllowTenantBypass && userPermissionClaims.Contains(PermissionClaimTypes.TenantFullAccess))
            || request.Roles.Any(role => userRoleClaims.Contains(role) || userPermissionClaims.Contains(role));
        return !isMatched ? ResultFailureFactory.Forbidden<TResponse>("You are not authorized.").WithErrorCode(AuthorizationErrorCodes.Forbidden) : await next(cancellationToken);
    }
}
