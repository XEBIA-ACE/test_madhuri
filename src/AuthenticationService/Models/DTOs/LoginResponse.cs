namespace AuthenticationService.Models.DTOs;

/// <summary>
/// Response model for successful login.
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// The authentication token (JWT).
    /// </summary>
    public required string Token { get; set; }

    /// <summary>
    /// When the token expires.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// The type of token (always "Bearer").
    /// </summary>
    public string TokenType { get; set; } = "Bearer";
}
