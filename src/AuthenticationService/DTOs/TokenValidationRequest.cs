using System.ComponentModel.DataAnnotations;

namespace AuthenticationService.DTOs;

/// <summary>
/// Request DTO for token validation.
/// </summary>
public class TokenValidationRequest
{
    [Required]
    public string Token { get; set; } = string.Empty;
}
