namespace AuthenticationService.DTOs;

/// <summary>
/// Response DTO for authentication operations.
/// </summary>
public class AuthResponse
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public string? Message { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
