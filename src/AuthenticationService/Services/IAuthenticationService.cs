using AuthenticationService.DTOs;

namespace AuthenticationService.Services;

/// <summary>
/// Interface for authentication operations.
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Registers a new user.
    /// </summary>
    Task<AuthResponse> RegisterAsync(RegisterRequest request);

    /// <summary>
    /// Authenticates a user and returns a token.
    /// </summary>
    Task<AuthResponse> LoginAsync(LoginRequest request);

    /// <summary>
    /// Logs out a user by invalidating their token.
    /// </summary>
    Task<AuthResponse> LogoutAsync(string token);

    /// <summary>
    /// Validates a token and returns user information.
    /// </summary>
    Task<TokenValidationResponse> ValidateTokenAsync(string token);
}
