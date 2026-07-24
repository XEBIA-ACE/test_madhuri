```csharp
namespace AuthenticationService.Configuration;

/// <summary>
/// Custom JWT settings specifically for SSO configurations.
/// </summary>
public class JwtSettings
{
    public const string SectionName = "JwtSettings";

    /// <summary>
    /// The SSO provider's base URL.
    /// </summary>
    public required string SSOProviderUrl { get; set; }

    /// <summary>
    /// List of URIs allowed for redirection after SSO.
    /// </summary>
    public required List<string> ValidRedirectUris { get; set; } = new List<string>();
}
```