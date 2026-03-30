using Application.Abstraction.Services;
using Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions;
public static class SecurityExtension
{
    public static IServiceCollection AddSecurityServices(this IServiceCollection services)
    {
        services.AddScoped<ISecurityService, SecurityService>();
        services.AddScoped<ITokenService, TokenService>();

        return services;
    }
}
