using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using Core.SecurityLayer.Encryptions;
using Core.SecurityLayer.Hashings;
using Core.SecurityLayer.JsonWebTokens.Abstractions;

using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Core.SecurityLayer.JsonWebTokens.Concretions;

public class JwtTokenHelper : IJwtTokenHelper
{
    private readonly TokenOption _tokenOptions;

    public JwtTokenHelper(IConfiguration configuration)
    {
        const string configurationSection = "TokenOptions";
        _tokenOptions = configuration.GetSection(configurationSection).Get<TokenOption>()
            ?? throw new InvalidOperationException($"\"{configurationSection}\" section cannot found in configuration.");
    }

    public AccessToken CreateToken(IEnumerable<Claim> claims)
    {
        var accessTokenExpiration = DateTime.UtcNow.AddMinutes(_tokenOptions.AccessTokenExpiration);
        SecurityKey securityKey = SecurityKeyHelper.CreateSecurityKey(_tokenOptions.SecurityKey);
        SigningCredentials signingCredentials = SigningCredentialHelper.CreateSigningCredentials(securityKey);

        JwtSecurityToken jwtSecurityToken = new(
            _tokenOptions.Issuer,
            _tokenOptions.Audience,
            expires: accessTokenExpiration,
            notBefore: DateTime.UtcNow,
            claims: claims,
            signingCredentials: signingCredentials);

        JwtSecurityTokenHandler jwtSecurityTokenHandler = new();
        string token = jwtSecurityTokenHandler.WriteToken(jwtSecurityToken);

        return new AccessToken { Token = token, Expiration = accessTokenExpiration };
    }

    public RefreshTokenResult CreateRefreshToken()
    {
        var rawToken = SecureTokenGenerator.GenerateUrlSafeToken();
        var expires = DateTime.UtcNow.AddMinutes(_tokenOptions.RefreshTokenTTL);
        return new RefreshTokenResult(rawToken, TokenHashingHelper.Hash(rawToken), expires);
    }
}
