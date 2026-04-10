namespace AuthenticationService.Models.Responses;

/// <summary>
/// Response model for token validation.
/// </summary>
public sealed class ValidateTokenResponse
{
    /// <summary>
    /// Indicates whether the token is valid.
    /// </summary>
    public required bool Valid { get; init; }

    /// <summary>
    /// The token expiration time in UTC (if valid).
    /// </summary>
    public DateTime? ExpiresAt { get; init; }
}
