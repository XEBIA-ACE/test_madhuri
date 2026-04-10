using System.ComponentModel.DataAnnotations;

namespace AuthenticationService.Models.DTOs;

/// <summary>
/// Request model for user logout.
/// </summary>
public class LogoutRequest
{
    /// <summary>
    /// The token to invalidate. If not provided, the token from the Authorization header will be used.
    /// </summary>
    public string? Token { get; set; }
}
