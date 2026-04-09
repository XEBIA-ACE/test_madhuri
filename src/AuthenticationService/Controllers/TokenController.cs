using AuthenticationService.DTOs;
using AuthenticationService.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuthenticationService.Controllers;

/// <summary>
/// Controller for token validation operations (internal service use).
/// </summary>
[ApiController]
[Route("api/token")]
public class TokenController : ControllerBase
{
    private readonly IAuthenticationService _authService;
    private readonly ILogger<TokenController> _logger;

    public TokenController(IAuthenticationService authService, ILogger<TokenController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Validates a token and returns user information.
    /// This endpoint is intended for internal service-to-service communication.
    /// </summary>
    /// <param name="request">Token validation request.</param>
    /// <returns>Token validation response with user info if valid.</returns>
    [HttpPost("validate")]
    [ProducesResponseType(typeof(TokenValidationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(TokenValidationResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Validate([FromBody] TokenValidationRequest request)
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(request.Token))
        {
            return BadRequest(new TokenValidationResponse
            {
                IsValid = false,
                Message = "Invalid request data."
            });
        }

        var result = await _authService.ValidateTokenAsync(request.Token);

        return Ok(result);
    }
}
