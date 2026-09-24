using Microsoft.IdentityModel.Tokens;

namespace Core.SecurityLayer.Encryptions;

public static class SigningCredentialHelper
{
    public static SigningCredentials CreateSigningCredentials(SecurityKey securityKey)
    {
        return new(securityKey, SecurityAlgorithms.HmacSha512Signature);
    }
}
