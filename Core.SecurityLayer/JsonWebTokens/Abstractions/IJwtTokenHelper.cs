using System.Security.Claims;

using Core.SecurityLayer.JsonWebTokens.Concretions;

namespace Core.SecurityLayer.JsonWebTokens.Abstractions;

public interface IJwtTokenHelper
{
    // Claims-based, not tied to a concrete entity type.
    AccessToken CreateToken(IEnumerable<Claim> claims);

    /// <summary>Persist <see cref="RefreshTokenResult.HashedToken"/>; never persist the raw value.</summary>
    RefreshTokenResult CreateRefreshToken();
}
