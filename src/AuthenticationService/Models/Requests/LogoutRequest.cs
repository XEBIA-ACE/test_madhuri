using System.ComponentModel.DataAnnotations;

namespace AuthenticationService.Models.Requests;

/// <summary>
/// Request model for user logout.
/// </summary>
public sealed class LogoutRequest
{
    /// <summary>
    /// The token to invalidate.
    /// </summary>
    [Required(ErrorMessage = "Token is required")]
    public required string Token { get; init; }
}
