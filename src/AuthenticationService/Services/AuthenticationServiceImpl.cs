using AuthenticationService.DTOs;
using AuthenticationService.Models;

namespace AuthenticationService.Services;

/// <summary>
/// Implementation of authentication operations.
/// </summary>
public class AuthenticationServiceImpl : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthenticationServiceImpl> _logger;

    public AuthenticationServiceImpl(
        IUserRepository userRepository,
        ISessionRepository sessionRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        ILogger<AuthenticationServiceImpl> logger)
    {
        _userRepository = userRepository;
        _sessionRepository = sessionRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        // Check if user already exists
        if (await _userRepository.ExistsAsync(request.Email))
        {
            _logger.LogWarning("Registration attempt with existing email: {Email}", request.Email);
            return new AuthResponse
            {
                Success = false,
                Message = "A user with this email already exists."
            };
        }

        // Create new user
        var user = new User
        {
            Email = request.Email,
            PasswordHash = _passwordHasher.HashPassword(request.Password)
        };

        await _userRepository.CreateAsync(user);
        _logger.LogInformation("User registered successfully: {UserId}", user.Id);

        // Generate token for immediate login
        var token = _tokenService.GenerateToken(user);
        var expiresAt = DateTime.UtcNow.AddHours(1); // TODO: Get from settings

        // Store session
        await _sessionRepository.CreateAsync(new Session
        {
            UserId = user.Id,
            Token = token,
            ExpiresAt = expiresAt
        });

        return new AuthResponse
        {
            Success = true,
            Token = token,
            ExpiresAt = expiresAt,
            Message = "Registration successful."
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        // Find user by email
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
        {
            _logger.LogWarning("Login attempt with non-existent email: {Email}", request.Email);
            return new AuthResponse
            {
                Success = false,
                Message = "Invalid email or password."
            };
        }

        // Verify password
        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login attempt with invalid password for user: {UserId}", user.Id);
            return new AuthResponse
            {
                Success = false,
                Message = "Invalid email or password."
            };
        }

        // Generate token
        var token = _tokenService.GenerateToken(user);
        var expiresAt = DateTime.UtcNow.AddHours(1); // TODO: Get from settings

        // Store session
        await _sessionRepository.CreateAsync(new Session
        {
            UserId = user.Id,
            Token = token,
            ExpiresAt = expiresAt
        });

        _logger.LogInformation("User logged in successfully: {UserId}", user.Id);

        return new AuthResponse
        {
            Success = true,
            Token = token,
            ExpiresAt = expiresAt,
            Message = "Login successful."
        };
    }

    public async Task<AuthResponse> LogoutAsync(string token)
    {
        var revoked = await _sessionRepository.RevokeByTokenAsync(token);
        
        if (!revoked)
        {
            _logger.LogWarning("Logout attempt with invalid or already revoked token");
            return new AuthResponse
            {
                Success = false,
                Message = "Invalid or already revoked token."
            };
        }

        _logger.LogInformation("User logged out successfully");

        return new AuthResponse
        {
            Success = true,
            Message = "Logout successful."
        };
    }

    public async Task<TokenValidationResponse> ValidateTokenAsync(string token)
    {
        // First validate the JWT signature and claims
        var jwtResult = _tokenService.ValidateToken(token);
        if (!jwtResult.IsValid)
        {
            return new TokenValidationResponse
            {
                IsValid = false,
                Message = jwtResult.ErrorMessage
            };
        }

        // Check if session is still valid (not revoked)
        var session = await _sessionRepository.GetByTokenAsync(token);
        if (session == null || session.IsRevoked || session.ExpiresAt < DateTime.UtcNow)
        {
            return new TokenValidationResponse
            {
                IsValid = false,
                Message = "Token has been revoked or expired."
            };
        }

        return new TokenValidationResponse
        {
            IsValid = true,
            UserId = jwtResult.UserId,
            Email = jwtResult.Email
        };
    }
}
