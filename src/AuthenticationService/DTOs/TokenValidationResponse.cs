namespace AuthenticationService.DTOs;

/// <summary>
/// Response DTO for token validation.
/// </summary>
public class TokenValidationResponse
{
    public bool IsValid { get; set; }
    public Guid? UserId { get; set; }
    public string? Email { get; set; }
    public string? Message { get; set; }
}
