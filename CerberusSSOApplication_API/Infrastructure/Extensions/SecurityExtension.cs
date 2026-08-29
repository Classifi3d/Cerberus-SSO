using Application.Abstraction.Services;
using Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions;
public static class SecurityExtension
{
    public static IServiceCollection AddSecurityServices(this IServiceCollection services)
    {
        services.AddScoped<ISecurityService, SecurityService>();

        services.AddSingleton<IOAuthSettings, OAuthSettings>();

        // Singleton, not scoped: the service owns the signing key, and resolving a new
        // one per request is what produced a fresh keypair on every call.
        services.AddSingleton<ITokenService, TokenService>();

        return services;
    }
}
