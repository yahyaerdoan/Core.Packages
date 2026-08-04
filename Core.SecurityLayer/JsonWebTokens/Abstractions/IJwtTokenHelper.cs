using Core.SecurityLayer.Entities;
using Core.SecurityLayer.JsonWebTokens.Concretions;

namespace Core.SecurityLayer.JsonWebTokens.Abstractions;

public interface IJwtTokenHelper
{
    AccessToken CreateToken(User user, IList<OperationClaim> operationClaims);

    /// <summary>
    /// Creates a refresh token whose <see cref="RefreshToken.Token"/> is already hashed for storage.
    /// The raw, unhashed value — the one to actually hand to the client — is returned separately and
    /// must never be persisted.
    /// </summary>
    (RefreshToken RefreshToken, string RawToken) CreateRefreshToken(User user, string ipAddress);
}
