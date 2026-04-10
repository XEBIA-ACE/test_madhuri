using System.ComponentModel.DataAnnotations;

namespace AuthenticationService.Models.Requests;

/// <summary>
/// Request model for user login.
/// </summary>
public sealed class LoginRequest
{
    /// <summary>
    /// The username for authentication.
    /// </summary>
    [Required(ErrorMessage = "Username is required")]
    [StringLength(256, MinimumLength = 1, ErrorMessage = "Username must be between 1 and 256 characters")]
    public required string Username { get; init; }

    /// <summary>
    /// The password for authentication.
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    [StringLength(256, MinimumLength = 1, ErrorMessage = "Password must be between 1 and 256 characters")]
    public required string Password { get; init; }
}
