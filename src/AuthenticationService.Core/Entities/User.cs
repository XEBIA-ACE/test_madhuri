namespace AuthenticationService.Core.Entities;

/// <summary>
/// Represents a user in the authentication system.
/// </summary>
public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation property
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
}
