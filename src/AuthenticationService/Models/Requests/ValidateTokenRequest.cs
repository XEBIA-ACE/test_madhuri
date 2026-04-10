using System.ComponentModel.DataAnnotations;

namespace AuthenticationService.Models.Requests;

/// <summary>
/// Request model for token validation.
/// </summary>
public sealed class ValidateTokenRequest
{
    /// <summary>
    /// The token to validate.
    /// </summary>
    [Required(ErrorMessage = "Token is required")]
    public required string Token { get; init; }
}
