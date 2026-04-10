namespace AuthenticationService.Configuration;

/// <summary>
/// Configuration settings for token management.
/// </summary>
public sealed class TokenSettings
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "TokenSettings";

    /// <summary>
    /// Token expiration time in minutes.
    /// </summary>
    public int TokenExpirationMinutes { get; set; } = 60;

    /// <summary>
    /// Whether to allow token refresh.
    /// </summary>
    public bool AllowRefresh { get; set; } = false;

    /// <summary>
    /// Maximum number of active tokens per user.
    /// </summary>
    public int MaxActiveTokensPerUser { get; set; } = 5;
}
