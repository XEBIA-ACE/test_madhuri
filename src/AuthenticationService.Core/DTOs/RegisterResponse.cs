namespace AuthenticationService.Core.DTOs;

/// <summary>
/// Response model for successful user registration.
/// </summary>
public class RegisterResponse
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Message { get; set; } = "Registration successful";
    public DateTime CreatedAt { get; set; }
}
