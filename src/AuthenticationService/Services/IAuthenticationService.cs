using AuthenticationService.Models.DTOs;

namespace AuthenticationService.Services;

/// <summary>
/// Service interface for authentication operations.
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Authenticates a user with the provided credentials.
    /// </summary>
    /// <param name="request">The login request containing credentials.</param>
    /// <returns>A tuple indicating success, the login response if successful, and error message if failed.</returns>
    Task<(bool Success, LoginResponse? Response, string? ErrorMessage)> LoginAsync(LoginRequest request);

    /// <summary>
    /// Logs out a user by invalidating their token.
    /// </summary>
    /// <param name="token">The token to invalidate.</param>
    /// <returns>A logout response indicating the result.</returns>
    Task<LogoutResponse> LogoutAsync(string token);

    /// <summary>
    /// Validates a token.
    /// </summary>
    /// <param name="request">The validation request containing the token.</param>
    /// <returns>A validation response with the result.</returns>
    Task<ValidateResponse> ValidateAsync(ValidateRequest request);
}
