namespace AuthenticationService.Configuration;

/// <summary>
/// Configuration settings for the external identity provider used in SSO authentication.
/// Values are loaded from environment variables (via IConfiguration) to avoid hardcoding
/// secrets and to allow changes without code redeployment.
///
/// Environment variable mapping (ASP.NET Core double-underscore convention):
///   IdentityProvider__ClientId
///   IdentityProvider__ClientSecret
///   IdentityProvider__RedirectUris   (semicolon-separated list)
///   IdentityProvider__AuthorizationEndpoint
///   IdentityProvider__TokenEndpoint
///   IdentityProvider__Issuer
/// </summary>
public class IdentityProviderSettings
{
    /// <summary>
    /// Configuration section name used in appsettings / environment variables.
    /// </summary>
    public const string SectionName = "IdentityProvider";

    /// <summary>
    /// OAuth 2.0 / OIDC client identifier issued by the identity provider.
    /// Maps to environment variable: IdentityProvider__ClientId
    /// </summary>
    public required string ClientId { get; set; }

    /// <summary>
    /// OAuth 2.0 / OIDC client secret issued by the identity provider.
    /// Maps to environment variable: IdentityProvider__ClientSecret
    /// </summary>
    public required string ClientSecret { get; set; }

    /// <summary>
    /// Pre-registered redirect URIs allowed in the authentication flow.
    /// Only URIs in this list will be accepted to mitigate Open Redirect attacks.
    /// Maps to environment variable: IdentityProvider__RedirectUris (semicolon-separated)
    /// </summary>
    public IList<string> RedirectUris { get; set; } = new List<string>();

    /// <summary>
    /// The identity provider's authorization endpoint URL.
    /// Maps to environment variable: IdentityProvider__AuthorizationEndpoint
    /// </summary>
    public string? AuthorizationEndpoint { get; set; }

    /// <summary>
    /// The identity provider's token endpoint URL.
    /// Maps to environment variable: IdentityProvider__TokenEndpoint
    /// </summary>
    public string? TokenEndpoint { get; set; }

    /// <summary>
    /// The expected issuer claim value for token validation.
    /// Maps to environment variable: IdentityProvider__Issuer
    /// </summary>
    public string? Issuer { get; set; }
}
