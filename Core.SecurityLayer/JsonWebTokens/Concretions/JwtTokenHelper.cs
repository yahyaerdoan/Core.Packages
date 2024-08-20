using Core.SecurityLayer.Encryptions;
using Core.SecurityLayer.Entities;
using Core.SecurityLayer.Extensions;
using Core.SecurityLayer.JsonWebTokens.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Core.SecurityLayer.JsonWebTokens.Concretions;

public class JwtTokenHelper : IJwtTokenHelper
{
    public IConfiguration Configuration { get; }
    private readonly TokenOption _tokenOptions;
    private DateTime _accessTokenExpiration;

    public JwtTokenHelper(IConfiguration configuration)
    {
        Configuration = configuration;
        const string configurationSection = "TokenOptions";
        _tokenOptions = Configuration.GetSection(configurationSection).Get<TokenOption>()
            ?? throw new NullReferenceException($"\"{configurationSection}\" section cannot found in configuration.");
    }

    public RefreshToken CreateRefreshToken(User user, string ipAddress)
    {
        RefreshToken refreshToken = new()
        {
            UserId = user.Id,
            Token = RandomRefreshToken(),
            Expires = DateTime.UtcNow.AddDays(7),
            CreatedByIp = ipAddress,
        };
        return refreshToken;
    }

    public AccessToken CreateToken(User user, IList<OperationClaim> operationClaims)
    {
        _accessTokenExpiration = DateTime.Now.AddMinutes(_tokenOptions.AccessTokenExpiration);
        SecurityKey securityKey = SecurityKeyHelper.CreateSecurityKey(_tokenOptions.SecurityKey);
        SigningCredentials signingCredentials = SigningCredentialHelper.CreateSigningCredentials(securityKey);
        JwtSecurityToken jwtSecurityToken = CreateJwtSecurityToken(_tokenOptions, user, signingCredentials, operationClaims);
        JwtSecurityTokenHandler jwtSecurityTokenHandler = new();
        string? token = jwtSecurityTokenHandler.WriteToken(jwtSecurityToken);

        return new AccessToken { Token = token, Expiration = _accessTokenExpiration };
    }

    public JwtSecurityToken CreateJwtSecurityToken(TokenOption tokenOptions, User user, SigningCredentials signingCredentials, IList<OperationClaim> operationClaims)
    {
        JwtSecurityToken jwtSecurityToken = new(
            tokenOptions.Issuer, 
            tokenOptions.Audience, 
            expires: _accessTokenExpiration, 
            notBefore: DateTime.Now, 
            claims: SetClaims(user, operationClaims), 
            signingCredentials: signingCredentials);
        return jwtSecurityToken;
    }

    private IEnumerable<Claim> SetClaims(User user, IList<OperationClaim> operationClaims)
    {
        List<Claim> claims = new();
        claims.AddNameIdentifier(user.Id.ToString());
        claims.AddEmail(user.Email);
        claims.AddName($"{user.FirstName} {user.LastName}");
        claims.AddRoles(operationClaims.Select(c => c.Name).ToArray());
        return claims;
    }

    private string RandomRefreshToken()
    {
        byte[] numberByte = new byte[32];
        using var random = RandomNumberGenerator.Create();
        random.GetBytes(numberByte);
        return Convert.ToBase64String(numberByte);
    }
}
