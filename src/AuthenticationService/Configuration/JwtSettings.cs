namespace AuthenticationService.Configuration;

/// <summary>
/// Configuration settings for JWT token generation and validation.
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// Configuration section name in appsettings.json.
    /// </summary>
    public const string SectionName = "JwtSettings";

    /// <summary>
    /// Secret key used for signing tokens.
    /// </summary>
    public required string SecretKey { get; set; }

    /// <summary>
    /// Token issuer (iss claim).
    /// </summary>
    public required string Issuer { get; set; }

    /// <summary>
    /// Token audience (aud claim).
    /// </summary>
    public required string Audience { get; set; }

    /// <summary>
    /// Token expiration time in minutes.
    /// </summary>
    public int ExpirationMinutes { get; set; } = 60;
}
