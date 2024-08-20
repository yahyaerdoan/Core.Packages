using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.SecurityLayer.Encryptions;

public static class SigningCredentialHelper
{
    public static SigningCredentials CreateSigningCredentials(SecurityKey securityKey) => new(securityKey, SecurityAlgorithms.HmacSha512Signature);
    
}
