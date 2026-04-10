using System.ComponentModel.DataAnnotations;

namespace AuthenticationService.Models.DTOs;

/// <summary>
/// Request model for token validation.
/// </summary>
public class ValidateRequest
{
    /// <summary>
    /// The token to validate.
    /// </summary>
    [Required(ErrorMessage = "Token is required")]
    public required string Token { get; set; }
}
