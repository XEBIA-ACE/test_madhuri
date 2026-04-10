using AuthenticationService.Models.Requests;
using AuthenticationService.Models.Responses;
using AuthenticationService.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationService.Controllers;

/// <summary>
/// API controller for authentication operations.
/// </summary>
[ApiController]
[Route("")]
[Produces("application/json")]
public sealed class AuthenticationController : ControllerBase
{
    private readonly IUserCredentialValidator _credentialValidator;
    private readonly ITokenManager _tokenManager;
    private readonly ILogger<AuthenticationController> _logger;

    public AuthenticationController(
        IUserCredentialValidator credentialValidator,
        ITokenManager tokenManager,
        ILogger<AuthenticationController> logger)
    {
        _credentialValidator = credentialValidator;
        _tokenManager = tokenManager;
        _logger = logger;
    }

    /// <summary>
    /// Authenticates a user and issues an authentication token.
    /// </summary>
    /// <param name="request">Login credentials.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Authentication token on success, error on failure.</returns>
    /// <response code="200">Login successful, token issued.</response>
    /// <response code="401">Invalid credentials.</response>
    /// <response code="400">Invalid request.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Login attempt for user: {Username}", request.Username);

        var validationResult = await _credentialValidator.ValidateAsync(
            request.Username,
            request.Password,
            cancellationToken);

        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Login failed for user: {Username}", request.Username);
            return Unauthorized(new ErrorResponse
            {
                ErrorCode = "AUTH_INVALID_CREDENTIALS",
                Message = validationResult.ErrorMessage ?? "Invalid username or password"
            });
        }

        var tokenInfo = await _tokenManager.IssueTokenAsync(validationResult.Username!, cancellationToken);

        _logger.LogInformation("Login successful for user: {Username}", request.Username);

        return Ok(new LoginResponse
        {
            Token = tokenInfo.Token,
            ExpiresAt = tokenInfo.ExpiresAt
        });
    }

    /// <summary>
    /// Validates an authentication token.
    /// </summary>
    /// <param name="request">Token to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Token validity status.</returns>
    /// <response code="200">Token validation result.</response>
    /// <response code="400">Invalid request.</response>
    [HttpPost("validate")]
    [ProducesResponseType(typeof(ValidateTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Validate(
        [FromBody] ValidateTokenRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Token validation request received");

        var tokenInfo = await _tokenManager.ValidateTokenAsync(request.Token, cancellationToken);

        if (tokenInfo == null)
        {
            _logger.LogDebug("Token validation failed: invalid or expired token");
            return Ok(new ValidateTokenResponse
            {
                Valid = false,
                ExpiresAt = null
            });
        }

        _logger.LogDebug("Token validated successfully for user: {Username}", tokenInfo.Username);

        return Ok(new ValidateTokenResponse
        {
            Valid = true,
            ExpiresAt = tokenInfo.ExpiresAt
        });
    }

    /// <summary>
    /// Logs out a user by invalidating their token.
    /// </summary>
    /// <param name="request">Token to invalidate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Logout confirmation.</returns>
    /// <response code="200">Logout successful.</response>
    /// <response code="400">Invalid request.</response>
    [HttpPost("logout")]
    [ProducesResponseType(typeof(LogoutResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Logout request received");

        var invalidated = await _tokenManager.InvalidateTokenAsync(request.Token, cancellationToken);

        if (invalidated)
        {
            _logger.LogInformation("Logout successful");
            return Ok(new LogoutResponse
            {
                Success = true,
                Message = "Logout successful"
            });
        }

        // Even if token not found, return success to prevent token enumeration
        _logger.LogDebug("Logout request for unknown token");
        return Ok(new LogoutResponse
        {
            Success = true,
            Message = "Logout successful"
        });
    }

    /// <summary>
    /// Health check endpoint.
    /// </summary>
    /// <returns>Service health status.</returns>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow });
    }
}
