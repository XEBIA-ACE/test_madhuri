using System.ComponentModel.DataAnnotations;

namespace AuthenticationService.Core.DTOs;

/// <summary>
/// Request model for token validation.
/// </summary>
public class TokenValidationRequest
{
    [Required(ErrorMessage = "Token is required")]
    public string Token { get; set; } = string.Empty;
}
