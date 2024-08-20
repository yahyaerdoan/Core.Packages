using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.SecurityLayer.JsonWebTokens.Concretions;

public class TokenOption
{
    public string Audience { get; set; }
    public string Issuer { get; set; }
    public int AccessTokenExpiration { get; set; }
    public string SecurityKey { get; set; }
    public int RefreshTokenTTL { get; set; }
    /// <summary>
    /// The term refreshTokenTTL typically refers to the "Time-To-Live" (TTL) of a refresh token in the context of authentication systems, particularly those using JWT (JSON Web Tokens).
    /// </summary>

    public TokenOption()
    {
        Audience = string.Empty;
        Issuer = string.Empty;
        SecurityKey = string.Empty;
    }

    public TokenOption(string audience, string issuer, int accessTokenExpiration, string securityKey, int refreshTokenTtl)
    {
        Audience = audience;
        Issuer = issuer;
        AccessTokenExpiration = accessTokenExpiration;
        SecurityKey = securityKey;
        RefreshTokenTTL = refreshTokenTtl;
    }
}
