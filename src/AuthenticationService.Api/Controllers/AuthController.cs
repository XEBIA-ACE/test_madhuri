using AuthenticationService.Core.DTOs;
using AuthenticationService.Core.Exceptions;
using AuthenticationService.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationService.Api.Controllers;

/// <summary>
/// Controller for authentication operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthenticationService authenticationService, ILogger<AuthController> logger)
    {
        _authenticationService = authenticationService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user.
    /// </summary>
    /// <param name="request">Registration details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Registration result</returns>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _authenticationService.RegisterAsync(request, cancellationToken);
            _logger.LogInformation("User registered successfully: {Email}", request.Email);
            return CreatedAtAction(nameof(Register), result);
        }
        catch (UserAlreadyExistsException ex)
        {
            _logger.LogWarning("Registration failed - user already exists: {Email}", request.Email);
            return Conflict(new ErrorResponse
            {
                Error = ex.Message,
                StatusCode = ex.StatusCode
            });
        }
        catch (AuthenticationException ex)
        {
            _logger.LogWarning("Registration failed: {Message}", ex.Message);
            return BadRequest(new ErrorResponse
            {
                Error = ex.Message,
                StatusCode = ex.StatusCode
            });
        }
    }

    /// <summary>
    /// Authenticate a user and return tokens.
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Authentication tokens</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _authenticationService.LoginAsync(request, cancellationToken);
            _logger.LogInformation("User logged in successfully: {Email}", request.Email);
            return Ok(result);
        }
        catch (InvalidCredentialsException ex)
        {
            _logger.LogWarning("Login failed - invalid credentials: {Email}", request.Email);
            return Unauthorized(new ErrorResponse
            {
                Error = ex.Message,
                StatusCode = ex.StatusCode
            });
        }
    }

    /// <summary>
    /// Logout a user and invalidate their token.
    /// </summary>
    /// <param name="request">Logout request with token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Logout result</returns>
    [HttpPost("logout")]
    [ProducesResponseType(typeof(LogoutResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken cancellationToken)
    {
        var result = await _authenticationService.LogoutAsync(request, cancellationToken);
        _logger.LogInformation("User logged out successfully");
        return Ok(result);
    }

    /// <summary>
    /// Validate a token for internal service use.
    /// </summary>
    /// <param name="request">Token validation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Token validation result</returns>
    [HttpPost("token/validate")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenValidationResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ValidateToken([FromBody] TokenValidationRequest request, CancellationToken cancellationToken)
    {
        var result = await _authenticationService.ValidateTokenAsync(request, cancellationToken);
        return Ok(result);
    }
}
