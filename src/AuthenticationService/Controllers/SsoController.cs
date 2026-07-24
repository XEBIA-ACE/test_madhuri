using AuthenticationService.Configuration;
using AuthenticationService.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace AuthenticationService.Controllers;

/// <summary>
/// Handles SSO (Single Sign-On) authentication flows.
/// Exposes an initiation endpoint that constructs and returns the
/// identity provider authorization URL for the HRIS login page.
/// </summary>
[ApiController]
[Route("api/auth/sso")]
public class SsoController : ControllerBase
{
    private readonly SsoSettings _ssoSettings;
    private readonly ILogger<SsoController> _logger;

    public SsoController(
        IOptions<SsoSettings> ssoSettings,
        ILogger<SsoController> logger)
    {
        _ssoSettings = ssoSettings.Value;
        _logger = logger;
    }

    /// <summary>
    /// Initiates the SSO authentication flow.
    ///
    /// Constructs the identity provider authorization URL using the
    /// pre-configured settings (client ID, redirect URI, scopes) and
    /// returns it to the caller. The caller (HRIS login page) is
    /// responsible for redirecting the browser to the returned URL.
    ///
    /// A cryptographically random <c>state</c> parameter is included in
    /// the authorization URL to mitigate CSRF attacks. The value is also
    /// stored in the response cookie so it can be validated when the IdP
    /// redirects back.
    ///
    /// Only the pre-registered <see cref="SsoSettings.RedirectUri"/> is
    /// used — custom redirect URIs are never accepted — which prevents
    /// Open Redirect vulnerabilities.
    /// </summary>
    /// <returns>
    /// 200 OK with a <see cref="SsoInitiateResponse"/> containing the
    /// authorization URL, or 503 if SSO is not configured.
    /// </returns>
    [HttpPost("initiate")]
    [ProducesResponseType(typeof(SsoInitiateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    public IActionResult Initiate()
    {
        // Guard: ensure SSO is properly configured before proceeding.
        if (string.IsNullOrWhiteSpace(_ssoSettings.AuthorizationEndpoint) ||
            string.IsNullOrWhiteSpace(_ssoSettings.ClientId) ||
            string.IsNullOrWhiteSpace(_ssoSettings.RedirectUri))
        {
            _logger.LogError(
                "SSO initiation requested but SsoSettings are incomplete. " +
                "Verify AuthorizationEndpoint, ClientId, and RedirectUri are configured.");

            return StatusCode(StatusCodes.Status503ServiceUnavailable, new ErrorResponse
            {
                ErrorCode = "SSO_NOT_CONFIGURED",
                Message = "Single Sign-On is not available at this time. Please contact your administrator."
            });
        }

        // Generate a cryptographically random state value to prevent CSRF.
        var state = GenerateState();

        // Persist state in a short-lived, HttpOnly, SameSite=Lax cookie so it
        // can be verified when the IdP redirects back to the callback endpoint.
        Response.Cookies.Append("sso_state", state, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            MaxAge = TimeSpan.FromMinutes(10),
            Path = "/"
        });

        // Build the authorization URL using only the pre-registered redirect URI.
        var authorizationUrl = BuildAuthorizationUrl(state);

        _logger.LogInformation(
            "SSO flow initiated. Redirecting to identity provider authorization endpoint.");

        return Ok(new SsoInitiateResponse { RedirectUrl = authorizationUrl });
    }

    // -----------------------------------------------------------------------
    // Private helpers
    // -----------------------------------------------------------------------

    /// <summary>
    /// Constructs the full authorization URL for the configured identity provider.
    /// </summary>
    private string BuildAuthorizationUrl(string state)
    {
        var query = HttpUtility.ParseQueryString(string.Empty);
        query["client_id"]     = _ssoSettings.ClientId;
        query["redirect_uri"]  = _ssoSettings.RedirectUri;   // pre-registered only
        query["response_type"] = _ssoSettings.ResponseType;
        query["scope"]         = _ssoSettings.Scope;
        query["state"]         = state;

        return $"{_ssoSettings.AuthorizationEndpoint.TrimEnd('/')}?{query}";
    }

    /// <summary>
    /// Generates a cryptographically secure random state string (Base64URL).
    /// </summary>
    private static string GenerateState()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        // Base64URL encode (no padding, URL-safe characters)
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
