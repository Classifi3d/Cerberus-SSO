using Application.Abstraction.Services;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services;

/// <inheritdoc />
public class OAuthSettings : IOAuthSettings
{
    public OAuthSettings(IConfiguration configuration)
    {
        LoginUrl = configuration["OAuth:LoginUrl"]
            ?? throw new InvalidOperationException(
                "OAuth:LoginUrl is not configured. It must be the absolute url of the login " +
                "page, for example http://localhost:4200/login.");

        Issuer = (configuration["JWT:Issuer"]
            ?? throw new InvalidOperationException(
                "JWT:Issuer is not configured. It must match the origin resource servers use " +
                "as their authority, for example http://localhost:5211."))
            .TrimEnd('/');
    }

    public string LoginUrl { get; }

    public string Issuer { get; }
}
