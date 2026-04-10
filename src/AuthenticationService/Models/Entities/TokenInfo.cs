namespace AuthenticationService.Models.Entities;

/// <summary>
/// Represents stored token information.
/// </summary>
public sealed class TokenInfo
{
    /// <summary>
    /// The token value.
    /// </summary>
    public required string Token { get; init; }

    /// <summary>
    /// The username associated with this token.
    /// </summary>
    public required string Username { get; init; }

    /// <summary>
    /// When the token was issued.
    /// </summary>
    public required DateTime IssuedAt { get; init; }

    /// <summary>
    /// When the token expires.
    /// </summary>
    public required DateTime ExpiresAt { get; init; }

    /// <summary>
    /// Whether the token has been invalidated (e.g., via logout).
    /// </summary>
    public bool IsInvalidated { get; set; }

    /// <summary>
    /// Checks if the token is currently valid (not expired and not invalidated).
    /// </summary>
    public bool IsValid => !IsInvalidated && DateTime.UtcNow < ExpiresAt;
}
