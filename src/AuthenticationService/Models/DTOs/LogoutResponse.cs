namespace AuthenticationService.Models.DTOs;

/// <summary>
/// Response model for logout operation.
/// </summary>
public class LogoutResponse
{
    /// <summary>
    /// Indicates whether the logout was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// A message describing the result.
    /// </summary>
    public required string Message { get; set; }
}
