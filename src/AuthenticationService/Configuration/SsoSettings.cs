namespace AuthenticationService.Configuration;

/// <summary>
/// Configuration settings for the SSO (Single Sign-On) identity provider.
/// Values are loaded from environment variables / appsettings to avoid
/// hard-coding sensitive data and to support environment-specific deployments.
/// </summary>
public class SsoSettings
{
    /// <summary>
    /// Configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "SsoSettings";

    /// <summary>
    /// The authorization endpoint URL of the identity provider (IdP).
    /// Example: https://idp.example.com/authorize
    /// </summary>
    public required string AuthorizationEndpoint { get; set; }

    /// <summary>
    /// The OAuth 2.0 / OIDC client ID registered with the identity provider.
    /// </summary>
    public required string ClientId { get; set; }

    /// <summary>
    /// The redirect URI that the identity provider will send the user back to
    /// after authentication. Must be pre-registered with the IdP to prevent
    /// Open Redirect attacks.
    /// </summary>
    public required string RedirectUri { get; set; }

    /// <summary>
    /// Space-separated list of OAuth scopes to request.
    /// Defaults to "openid profile email".
    /// </summary>
    public string Scope { get; set; } = "openid profile email";

    /// <summary>
    /// OAuth 2.0 response type. Defaults to "code" (Authorization Code flow).
    /// </summary>
    public string ResponseType { get; set; } = "code";
}
