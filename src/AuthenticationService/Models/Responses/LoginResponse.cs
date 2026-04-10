namespace AuthenticationService.Models.Responses;

/// <summary>
/// Response model for successful login.
/// </summary>
public sealed class LoginResponse
{
    /// <summary>
    /// The authentication token.
    /// </summary>
    public required string Token { get; init; }

    /// <summary>
    /// The token expiration time in UTC.
    /// </summary>
    public required DateTime ExpiresAt { get; init; }
}
