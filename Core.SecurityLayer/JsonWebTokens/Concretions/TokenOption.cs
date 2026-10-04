namespace Core.SecurityLayer.JsonWebTokens.Concretions;

/// <summary>JWT settings bound from the "TokenOptions" section by AddTokenOptions.</summary>
public class TokenOption
{
    public const string SectionName = "TokenOptions";

    /// <summary>HMAC-SHA512 signing needs a key of at least 64 bytes.</summary>
    public const int MinSecurityKeyBytes = 64;

    public string Audience { get; set; }

    public string Issuer { get; set; }

    public int AccessTokenExpiration { get; set; }

    public string SecurityKey { get; set; }

    /// <summary>Refresh token time-to-live, in minutes.</summary>
    public int RefreshTokenTTL { get; set; }

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
