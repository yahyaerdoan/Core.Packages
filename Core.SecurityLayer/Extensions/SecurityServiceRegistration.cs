using Core.SecurityLayer.JsonWebTokens.Abstractions;
using Core.SecurityLayer.JsonWebTokens.Concretions;
using Core.SecurityLayer.OneTimePasswordAuthenticators.Concretions.OneTimePasswordNet;
using Core.SecurityLayer.OptAuthenticators.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
