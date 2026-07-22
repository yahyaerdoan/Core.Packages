using Core.ApplicationLayer.Pipelines.Authorizations.Abstractions;
using Core.CrossCuttingConcernLayer.ExceptionHandlings.Types.Authorizations;
using Core.SecurityLayer.Constants;
using Core.SecurityLayer.Extensions;

using MediatR;

using Microsoft.AspNetCore.Http;

namespace Core.ApplicationLayer.Pipelines.Authorizations.Concretions;

public class AuthorizationAddingBehavior<TRequest, TResponse>(IHttpContextAccessor httpContextAccessor) : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>, ISecureAddRequest
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        List<string>? userRoleClaims = httpContextAccessor.HttpContext.User.ClaimRoles() ?? throw new AuthorizationException("You are not authenticated.");

        bool isNotMatchedAUserRoleClaimWithRequestRoles =
            string.IsNullOrEmpty(userRoleClaims
            .FirstOrDefault(userRoleClaim => userRoleClaim == GeneralOperationClaims.Admin || request.Roles.Any(role => role == userRoleClaim)));

        if (isNotMatchedAUserRoleClaimWithRequestRoles)
            throw new AuthorizationException("You are not authorized.");

        TResponse response = await next(cancellationToken);
        return response;
    }
}
