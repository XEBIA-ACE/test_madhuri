namespace AuthenticationService.Core.DTOs;

/// <summary>
/// Response model for token validation.
/// </summary>
public class TokenValidationResponse
{
    public bool IsValid { get; set; }
    public Guid? UserId { get; set; }
    public string? Email { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public string? Message { get; set; }
}
