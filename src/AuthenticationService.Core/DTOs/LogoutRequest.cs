using System.ComponentModel.DataAnnotations;

namespace AuthenticationService.Core.DTOs;

/// <summary>
/// Request model for user logout.
/// </summary>
public class LogoutRequest
{
    [Required(ErrorMessage = "Token is required")]
    public string Token { get; set; } = string.Empty;
}
