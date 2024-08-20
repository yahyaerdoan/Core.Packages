using Core.SecurityLayer.Entities;
using Core.SecurityLayer.JsonWebTokens.Concretions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.SecurityLayer.JsonWebTokens.Abstractions;

public interface IJwtTokenHelper
{
    AccessToken CreateToken(User user, IList<OperationClaim> operationClaims);
    RefreshToken CreateRefreshToken(User user, string ipAddress);
}
