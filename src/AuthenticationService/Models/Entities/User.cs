namespace AuthenticationService.Models.Entities;

/// <summary>
/// Represents a user entity for authentication purposes.
/// </summary>
public class User
{
    /// <summary>
    /// Unique identifier for the user.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Username for authentication.
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// Hashed password for credential verification.
    /// </summary>
    public required string PasswordHash { get; set; }

    /// <summary>
    /// Indicates whether the user account is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Timestamp when the user was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
