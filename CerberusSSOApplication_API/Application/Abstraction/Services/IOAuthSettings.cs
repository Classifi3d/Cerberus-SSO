namespace Application.Abstraction.Services;

/// <summary>
/// OAuth deployment settings the application layer needs but must not read from
/// configuration itself.
/// </summary>
public interface IOAuthSettings
{
    /// <summary>
    /// Absolute url of the interactive login page, for example
    /// http://localhost:4200/login. The authorize endpoint redirects the browser here
    /// with a requestId; a relative path would resolve against the API's own origin,
    /// where no login page exists.
    /// </summary>
    string LoginUrl { get; }

    /// <summary>Issuer value stamped into tokens and published by the discovery document.</summary>
    string Issuer { get; }
}
