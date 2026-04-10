namespace AuthenticationService.Models.Entities;

/// <summary>
/// Represents user credentials stored in the user data store.
/// </summary>
public sealed class UserCredentials
{
    /// <summary>
    /// Unique user identifier.
    /// </summary>
    public required string UserId { get; init; }

    /// <summary>
    /// The username.
    /// </summary>
    public required string Username { get; init; }

    /// <summary>
    /// The hashed password.
    /// </summary>
    public required string PasswordHash { get; init; }

    /// <summary>
    /// Whether the user account is active.
    /// </summary>
    public bool IsActive { get; init; } = true;

    /// <summary>
    /// When the user was created.
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
