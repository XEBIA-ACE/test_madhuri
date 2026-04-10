namespace AuthenticationService.Models.DTOs;

/// <summary>
/// Response model for token validation.
/// </summary>
public class ValidateResponse
{
    /// <summary>
    /// Indicates whether the token is valid.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// The user ID associated with the token, if valid.
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// The username associated with the token, if valid.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// When the token expires, if valid.
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Error message if the token is invalid.
    /// </summary>
    public string? ErrorMessage { get; set; }
}
