namespace AuthenticationService.Models.Entities;

/// <summary>
/// Represents a stored token record for session management and invalidation.
/// </summary>
public class TokenRecord
{
    /// <summary>
    /// Unique identifier for the token record.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The JWT token identifier (jti claim).
    /// </summary>
    public required string TokenId { get; set; }

    /// <summary>
    /// The user ID associated with this token.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// When the token expires.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// When the token was issued.
    /// </summary>
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Indicates whether the token has been invalidated (e.g., via logout).
    /// </summary>
    public bool IsInvalidated { get; set; } = false;

    /// <summary>
    /// When the token was invalidated, if applicable.
    /// </summary>
    public DateTime? InvalidatedAt { get; set; }
}
