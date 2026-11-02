using ResultHandler.Core.Enums;
using ResultHandler.ErrorCodes;

namespace Core.ApplicationLayer.Pipelines.Authorizations.Constants;

public static class AuthorizationErrorCodes
{
    [ErrorCode(ResultStatus.Forbidden, "The caller is signed in but lacks the permission the endpoint needs.", General = true)]
    public const string Forbidden = "Auth.Forbidden";

    [ErrorCode(ResultStatus.Unauthorized, "The request has no valid access token; sign in or refresh the token.", General = true)]
    public const string Unauthenticated = "Auth.Unauthenticated";
}
