using Core.SecurityLayer.Entities;
using Core.SecurityLayer.JsonWebTokens.Concretions;

namespace Core.SecurityLayer.JsonWebTokens.Abstractions;

public interface IJwtTokenHelper
{
    AccessToken CreateToken(User user, IList<OperationClaim> operationClaims);
    RefreshToken CreateRefreshToken(User user, string ipAddress);
}
