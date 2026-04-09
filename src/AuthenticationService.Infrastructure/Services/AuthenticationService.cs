using AuthenticationService.Core.DTOs;
using AuthenticationService.Core.Entities;
using AuthenticationService.Core.Exceptions;
using AuthenticationService.Core.Interfaces;
using AuthenticationService.Core.Configuration;
using Microsoft.Extensions.Options;

namespace AuthenticationService.Infrastructure.Services;

/// <summary>
/// Implementation of the authentication service.
/// </summary>
public class AuthenticationServiceImpl : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly JwtSettings _jwtSettings;

    public AuthenticationServiceImpl(
        IUserRepository userRepository,
        ISessionRepository sessionRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IOptions<JwtSettings> jwtSettings)
    {
        _userRepository = userRepository;
        _sessionRepository = sessionRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        // Check if user already exists
        if (await _userRepository.ExistsAsync(request.Email, cancellationToken))
        {
            throw new UserAlreadyExistsException(request.Email);
        }

        // Create new user
        var user = new User
        {
            Email = request.Email.ToLower().Trim(),
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            IsActive = true
        };

        var createdUser = await _userRepository.CreateAsync(user, cancellationToken);

        return new RegisterResponse
        {
            UserId = createdUser.Id,
            Email = createdUser.Email,
            Message = "Registration successful",
            CreatedAt = createdUser.CreatedAt
        };
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        // Find user by email
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user == null || !user.IsActive)
        {
            throw new InvalidCredentialsException();
        }

        // Verify password
        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new InvalidCredentialsException();
        }

        // Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var expiresAt = _tokenService.GetAccessTokenExpiration();

        // Create session
        var session = new Session
        {
            UserId = user.Id,
            Token = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays)
        };

        await _sessionRepository.CreateAsync(session, cancellationToken);

        return new LoginResponse
        {
            UserId = user.Id,
            Email = user.Email,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            TokenType = "Bearer"
        };
    }

    public async Task<LogoutResponse> LogoutAsync(LogoutRequest request, CancellationToken cancellationToken = default)
    {
        // Find session by token
        var session = await _sessionRepository.GetByTokenAsync(request.Token, cancellationToken);

        if (session == null)
        {
            // Token not found, but we still return success for security reasons
            return new LogoutResponse
            {
                Success = true,
                Message = "Logout successful"
            };
        }

        // Revoke the session
        session.RevokedAt = DateTime.UtcNow;
        await _sessionRepository.UpdateAsync(session, cancellationToken);

        return new LogoutResponse
        {
            Success = true,
            Message = "Logout successful"
        };
    }

    public async Task<TokenValidationResponse> ValidateTokenAsync(TokenValidationRequest request, CancellationToken cancellationToken = default)
    {
        // Validate JWT structure and signature
        var (isValid, userId, email) = _tokenService.ValidateAccessToken(request.Token);

        if (!isValid || !userId.HasValue)
        {
            return new TokenValidationResponse
            {
                IsValid = false,
                Message = "Invalid or expired token"
            };
        }

        // Check if session exists and is not revoked
        var session = await _sessionRepository.GetByTokenAsync(request.Token, cancellationToken);

        if (session == null || session.IsRevoked)
        {
            return new TokenValidationResponse
            {
                IsValid = false,
                Message = "Token has been revoked"
            };
        }

        // Check if user is still active
        var user = await _userRepository.GetByIdAsync(userId.Value, cancellationToken);

        if (user == null || !user.IsActive)
        {
            return new TokenValidationResponse
            {
                IsValid = false,
                Message = "User account is inactive"
            };
        }

        return new TokenValidationResponse
        {
            IsValid = true,
            UserId = userId,
            Email = email,
            ExpiresAt = session.ExpiresAt,
            Message = "Token is valid"
        };
    }
}
