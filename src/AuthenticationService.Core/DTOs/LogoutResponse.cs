namespace AuthenticationService.Core.DTOs;

/// <summary>
/// Response model for successful logout.
/// </summary>
public class LogoutResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = "Logout successful";
}
