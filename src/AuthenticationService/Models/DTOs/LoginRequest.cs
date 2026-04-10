using System.ComponentModel.DataAnnotations;

namespace AuthenticationService.Models.DTOs;

/// <summary>
/// Request model for user login.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// The username for authentication.
    /// </summary>
    [Required(ErrorMessage = "Username is required")]
    public required string Username { get; set; }

    /// <summary>
    /// The password for authentication.
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    public required string Password { get; set; }
}
