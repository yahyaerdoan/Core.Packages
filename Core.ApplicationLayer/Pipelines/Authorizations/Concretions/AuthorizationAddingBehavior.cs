using Core.ApplicationLayer.Pipelines.Authorizations.Abstractions;
using Core.ApplicationLayer.Pipelines.Authorizations.Constants;
using Core.ApplicationLayer.Pipelines.Authorizations.Extensions;
using MediatR;
using Microsoft.AspNetCore.Http;
using ResultHandler.Core.Abstractions;
using ResultHandler.Facade;
using ResultHandler.Functional;

namespace Core.ApplicationLayer.Pipelines.Authorizations.Concretions;

public class AuthorizationAddingBehavior<TRequest, TResponse>(IHttpContextAccessor httpContextAccessor)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, ISecureAddRequest
    where TResponse : class, IOperationResult, IResultFailureFactory<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var user = httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated is not true)
        {
            return ResultFailureFactory.Unauthorized<TResponse>("You are not authenticated.").WithCode(AuthorizationErrorCodes.Unauthenticated);
        }

        return !user.IsAuthorizedFor(request)
            ? ResultFailureFactory.Forbidden<TResponse>("You are not authorized.").WithCode(AuthorizationErrorCodes.Forbidden)
            : await next(cancellationToken);
    }
}
