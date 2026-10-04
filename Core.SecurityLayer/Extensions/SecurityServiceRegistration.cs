using Core.SecurityLayer.JsonWebTokens.Abstractions;
using Core.SecurityLayer.JsonWebTokens.Concretions;
using Microsoft.AspNetCore.Identity;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Core.SecurityLayer.Extensions;

public static class SecurityServiceRegistration
{
    public static IServiceCollection AddSecurityServices(this IServiceCollection services)
    {
        _ = services.AddSingleton<IJwtTokenHelper, JwtTokenHelper>();
        return services;
    }

    /// <summary>Binds <see cref="TokenOption"/> from "TokenOptions" once; the app fails at startup when the key is shorter than 64 bytes or a value is missing.</summary>
    public static IServiceCollection AddTokenOptions(this IServiceCollection services, IConfiguration configuration)
    {
        _ = services.AddOptions<TokenOption>()
            .Bind(configuration.GetSection(TokenOption.SectionName))
            .Validate(o => Encoding.UTF8.GetByteCount(o.SecurityKey ?? string.Empty) >= TokenOption.MinSecurityKeyBytes,
                $"{TokenOption.SectionName}:{nameof(TokenOption.SecurityKey)} must be at least {TokenOption.MinSecurityKeyBytes} bytes; keep it in user-secrets or a secret store.")
            .Validate(o => !string.IsNullOrWhiteSpace(o.Issuer) && !string.IsNullOrWhiteSpace(o.Audience),
                $"{TokenOption.SectionName}:{nameof(TokenOption.Issuer)} and {nameof(TokenOption.Audience)} are required.")
            .Validate(o => o.AccessTokenExpiration > 0 && o.RefreshTokenTTL > 0,
                $"{TokenOption.SectionName}:{nameof(TokenOption.AccessTokenExpiration)} and {nameof(TokenOption.RefreshTokenTTL)} must be greater than 0.")
            .ValidateOnStart();
        return services;
    }

    // AddIdentityCore (not AddIdentity) — this API is controller+JWT based, no cookie auth needed.
    public static IServiceCollection AddCoreIdentity<TUser, TRole, TKey, TContext>(this IServiceCollection services)
        where TUser : IdentityUser<TKey>
        where TRole : IdentityRole<TKey>
        where TKey : IEquatable<TKey>
        where TContext : DbContext
    {
        _ = services.AddIdentityCore<TUser>()
            .AddRoles<TRole>()
            .AddEntityFrameworkStores<TContext>()
            .AddDefaultTokenProviders();

        return services;
    }
}
