namespace AuthenticationService.Models.Responses;

/// <summary>
/// Response model for successful logout.
/// </summary>
public sealed class LogoutResponse
{
    /// <summary>
    /// Confirmation message for logout.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Indicates whether the logout was successful.
    /// </summary>
    public required bool Success { get; init; }
}
