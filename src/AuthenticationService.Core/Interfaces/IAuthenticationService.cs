using AuthenticationService.Core.DTOs;

namespace AuthenticationService.Core.Interfaces;

/// <summary>
/// Service interface for authentication operations.
/// </summary>
public interface IAuthenticationService
{
    Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<LogoutResponse> LogoutAsync(LogoutRequest request, CancellationToken cancellationToken = default);
    Task<TokenValidationResponse> ValidateTokenAsync(TokenValidationRequest request, CancellationToken cancellationToken = default);
}
