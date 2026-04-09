namespace AuthenticationService.Core.Entities;

/// <summary>
/// Represents a user session for token management.
/// </summary>
public class Session
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public bool IsRevoked => RevokedAt.HasValue;

    // Navigation property
    public User? User { get; set; }
}
