using Core.SecurityLayer.JsonWebTokens.Abstractions;
using Core.SecurityLayer.JsonWebTokens.Concretions;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Core.SecurityLayer.Extensions;

public static class SecurityServiceRegistration
{
    public static IServiceCollection AddSecurityServices(this IServiceCollection services)
    {
        services.AddScoped<IJwtTokenHelper, JwtTokenHelper>();
        return services;
    }

    // AddIdentityCore (not AddIdentity) — this API is controller+JWT based, no cookie auth needed.
    public static IServiceCollection AddCoreIdentity<TUser, TRole, TKey, TContext>(this IServiceCollection services)
        where TUser : IdentityUser<TKey>
        where TRole : IdentityRole<TKey>
        where TKey : IEquatable<TKey>
        where TContext : DbContext
    {
        services.AddIdentityCore<TUser>()
            .AddRoles<TRole>()
            .AddEntityFrameworkStores<TContext>()
            .AddDefaultTokenProviders();

        return services;
    }
}
