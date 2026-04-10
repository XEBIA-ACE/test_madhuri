using AuthenticationService.Models.DTOs;
using AuthenticationService.Repositories;

namespace AuthenticationService.Services;

/// <summary>
/// Implementation of the authentication service.
/// </summary>
public class AuthenticationServiceImpl : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthenticationServiceImpl> _logger;

    public AuthenticationServiceImpl(
        IUserRepository userRepository,
        ITokenService tokenService,
        ILogger<AuthenticationServiceImpl> logger)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<(bool Success, LoginResponse? Response, string? ErrorMessage)> LoginAsync(LoginRequest request)
    {
        _logger.LogInformation("Login attempt for user: {Username}", request.Username);

        // Retrieve user by username
        var user = await _userRepository.GetByUsernameAsync(request.Username);
        if (user == null)
        {
            _logger.LogWarning("Login failed: User not found - {Username}", request.Username);
            return (false, null, "Invalid username or password");
        }

        // Verify password
        if (!_userRepository.VerifyPassword(user, request.Password))
        {
            _logger.LogWarning("Login failed: Invalid password for user - {Username}", request.Username);
            return (false, null, "Invalid username or password");
        }

        // Generate token
        var (token, expiresAt) = await _tokenService.GenerateTokenAsync(user);

        _logger.LogInformation("Login successful for user: {Username}, UserId: {UserId}", request.Username, user.Id);

        var response = new LoginResponse
        {
            Token = token,
            ExpiresAt = expiresAt
        };

        return (true, response, null);
    }

    /// <inheritdoc />
    public async Task<LogoutResponse> LogoutAsync(string token)
    {
        _logger.LogInformation("Logout attempt");

        var invalidated = await _tokenService.InvalidateTokenAsync(token);

        if (invalidated)
        {
            _logger.LogInformation("Logout successful - token invalidated");
            return new LogoutResponse
            {
                Success = true,
                Message = "Successfully logged out"
            };
        }

        // Even if token wasn't found in store, consider logout successful
        // (token may have already expired or been invalidated)
        _logger.LogInformation("Logout completed - token may have already been invalidated or expired");
        return new LogoutResponse
        {
            Success = true,
            Message = "Logout completed"
        };
    }

    /// <inheritdoc />
    public async Task<ValidateResponse> ValidateAsync(ValidateRequest request)
    {
        _logger.LogDebug("Token validation request");

        var (isValid, userId, username, expiresAt, errorMessage) = await _tokenService.ValidateTokenAsync(request.Token);

        if (isValid)
        {
            _logger.LogDebug("Token validation successful for user: {Username}", username);
            return new ValidateResponse
            {
                IsValid = true,
                UserId = userId,
                Username = username,
                ExpiresAt = expiresAt
            };
        }

        _logger.LogDebug("Token validation failed: {ErrorMessage}", errorMessage);
        return new ValidateResponse
        {
            IsValid = false,
            ErrorMessage = errorMessage
        };
    }
}
