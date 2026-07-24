using System.Security.Cryptography;
using AuthenticationService.Configuration;
using AuthenticationService.Models.DTOs;
using AuthenticationService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AuthenticationService.Controllers;

/// <summary>
/// Handles authentication operations: credential-based login/logout, token validation,
/// and SSO (Single Sign-On) initiation.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthenticationController : ControllerBase
{
    private readonly IUserCredentialValidator _credentialValidator;
    private readonly ITokenManager _tokenManager;
    private readonly IAuthenticationMetrics _metrics;
    private readonly SsoSettings _ssoSettings;
    private readonly ILogger<AuthenticationController> _logger;

    public AuthenticationController(
        IUserCredentialValidator credentialValidator,
        ITokenManager tokenManager,
        IAuthenticationMetrics metrics,
        IOptions<SsoSettings> ssoSettings,
        ILogger<AuthenticationController> logger)
    {
        _credentialValidator = credentialValidator;
        _tokenManager = tokenManager;
        _metrics = metrics;
        _ssoSettings = ssoSettings.Value;
        _logger = logger;
    }

    // -------------------------------------------------------------------------
    // POST /api/authentication/login
    // -------------------------------------------------------------------------

    /// <summary>
    /// Authenticates a user with username and password credentials.
    /// </summary>
    /// <param name="request">Login credentials.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A bearer token on success.</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ErrorResponse
            {
                ErrorCode = "VALIDATION_ERROR",
                Message = "Invalid request payload."
            });
        }

        var isValid = await _credentialValidator.ValidateAsync(
            request.Username, request.Password, cancellationToken);

        if (!isValid)
        {
            _logger.LogWarning("Failed login attempt for user: {Username}", request.Username);
            _metrics.RecordFailedLogin(request.Username);

            return Unauthorized(new ErrorResponse
            {
                ErrorCode = "INVALID_CREDENTIALS",
                Message = "The username or password is incorrect."
            });
        }

        var tokenInfo = await _tokenManager.IssueTokenAsync(request.Username, cancellationToken);

        _logger.LogInformation("Successful login for user: {Username}", request.Username);
        _metrics.RecordSuccessfulLogin(request.Username);

        return Ok(new LoginResponse
        {
            Token = tokenInfo.Token,
            ExpiresAt = tokenInfo.ExpiresAt
        });
    }

    // -------------------------------------------------------------------------
    // POST /api/authentication/logout
    // -------------------------------------------------------------------------

    /// <summary>
    /// Invalidates the supplied bearer token (logout).
    /// </summary>
    /// <param name="request">Logout request containing the token to invalidate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutRequest request,
        CancellationToken cancellationToken = default)
    {
        // Accept token from body or Authorization header
        var token = request.Token
            ?? HttpContext.Request.Headers.Authorization
                .FirstOrDefault()
                ?.Replace("Bearer ", string.Empty, StringComparison.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(token))
        {
            return BadRequest(new ErrorResponse
            {
                ErrorCode = "MISSING_TOKEN",
                Message = "A token must be provided either in the request body or the Authorization header."
            });
        }

        await _tokenManager.InvalidateTokenAsync(token, cancellationToken);

        _logger.LogInformation("Token invalidated (logout).");
        return NoContent();
    }

    // -------------------------------------------------------------------------
    // POST /api/authentication/validate
    // -------------------------------------------------------------------------

    /// <summary>
    /// Validates a bearer token and returns its metadata.
    /// </summary>
    /// <param name="token">The token string to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    [HttpGet("validate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Validate(
        [FromQuery] string token,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Unauthorized(new ErrorResponse
            {
                ErrorCode = "MISSING_TOKEN",
                Message = "Token is required."
            });
        }

        var tokenInfo = await _tokenManager.ValidateTokenAsync(token, cancellationToken);

        if (tokenInfo == null)
        {
            return Unauthorized(new ErrorResponse
            {
                ErrorCode = "INVALID_TOKEN",
                Message = "The token is invalid or has expired."
            });
        }

        return Ok(new
        {
            tokenInfo.Username,
            tokenInfo.IssuedAt,
            tokenInfo.ExpiresAt
        });
    }

    // -------------------------------------------------------------------------
    // GET /api/authentication/sso/initiate
    // -------------------------------------------------------------------------

    /// <summary>
    /// Initiates a Single Sign-On (SSO) authentication flow using the configured
    /// identity provider settings.
    ///
    /// The endpoint constructs a standards-compliant OAuth 2.0 / OIDC authorization
    /// request URL using only the pre-registered redirect URI from environment
    /// configuration, preventing Open Redirect vulnerabilities.
    ///
    /// The caller should redirect the user's browser to the returned
    /// <c>authorizationUrl</c> and persist the returned <c>state</c> value so it
    /// can be verified when the identity provider redirects back.
    /// </summary>
    /// <returns>
    /// 200 OK with <see cref="SsoInitiateResponse"/> containing the authorization URL
    /// and CSRF state token.
    /// 503 Service Unavailable if SSO is not configured.
    /// </returns>
    [HttpGet("sso/initiate")]
    [ProducesResponseType(typeof(SsoInitiateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    public IActionResult InitiateSso()
    {
        // Guard: ensure SSO has been configured in the environment.
        if (string.IsNullOrWhiteSpace(_ssoSettings.AuthorizationEndpoint)
            || string.IsNullOrWhiteSpace(_ssoSettings.ClientId)
            || string.IsNullOrWhiteSpace(_ssoSettings.RedirectUri))
        {
            _logger.LogError(
                "SSO initiation requested but identity provider settings are not fully configured.");

            return StatusCode(StatusCodes.Status503ServiceUnavailable, new ErrorResponse
            {
                ErrorCode = "SSO_NOT_CONFIGURED",
                Message = "Single Sign-On is not available. Identity provider settings are missing."
            });
        }

        // Generate a cryptographically random state value to prevent CSRF.
        // The client must store this and verify it matches the value returned
        // by the identity provider in the callback.
        var state = GenerateState();

        // Build the authorization URL using only the pre-registered redirect URI
        // from configuration — custom/dynamic redirect URIs are explicitly rejected
        // per the spec (mitigates Open Redirect).
        var authorizationUrl = BuildAuthorizationUrl(state);

        _logger.LogInformation(
            "SSO initiation requested. Redirecting to identity provider: {Endpoint}",
            _ssoSettings.AuthorizationEndpoint);

        return Ok(new SsoInitiateResponse
        {
            AuthorizationUrl = authorizationUrl,
            State = state
        });
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Constructs the OAuth 2.0 / OIDC authorization request URL from the
    /// identity provider settings stored in configuration.
    /// Only the pre-registered <see cref="SsoSettings.RedirectUri"/> is used.
    /// </summary>
    private string BuildAuthorizationUrl(string state)
    {
        // Use UriBuilder + query string construction to avoid manual string
        // concatenation errors and ensure proper URL encoding.
        var queryParams = new Dictionary<string, string>
        {
            ["response_type"] = _ssoSettings.ResponseType,
            ["client_id"]     = _ssoSettings.ClientId,
            // Only the pre-registered redirect URI from configuration is used here.
            ["redirect_uri"]  = _ssoSettings.RedirectUri,
            ["scope"]         = _ssoSettings.Scope,
            ["state"]         = state
        };

        var queryString = string.Join("&", queryParams.Select(
            kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));

        var separator = _ssoSettings.AuthorizationEndpoint.Contains('?') ? "&" : "?";
        return $"{_ssoSettings.AuthorizationEndpoint}{separator}{queryString}";
    }

    /// <summary>
    /// Generates a cryptographically secure random state string (Base64Url-encoded).
    /// </summary>
    private static string GenerateState()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        // Base64Url encode: replace '+' with '-', '/' with '_', strip padding.
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }
}
