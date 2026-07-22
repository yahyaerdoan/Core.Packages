using Core.SecurityLayer.JsonWebTokens.Abstractions;
using Core.SecurityLayer.JsonWebTokens.Concretions;
using Core.SecurityLayer.OneTimePasswordAuthenticators.Abstractions;
using Core.SecurityLayer.OneTimePasswordAuthenticators.Concretions.OneTimePasswordNet;

using Microsoft.Extensions.DependencyInjection;

namespace Core.SecurityLayer.Extensions;

public static class SecurityServiceRegistration
{
    public static IServiceCollection AddSecurityServices(this IServiceCollection services)
    {
        services.AddScoped<IJwtTokenHelper, JwtTokenHelper>();
        services.AddScoped<IOneTimePasswordAuthenticatorHelper, OtpNetOneTimePasswordAuthenticatorHelper>();
        //services.AddScoped<IEmailAuthenticatorHelper, EmailAuthenticatorHelper>();
        return services;
    }
}
