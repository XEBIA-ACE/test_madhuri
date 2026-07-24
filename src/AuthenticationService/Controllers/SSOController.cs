```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using AuthenticationService.Configuration;
using AuthenticationService.Services;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Claims;

namespace AuthenticationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SSOController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly JwtSettings _jwtSettings;

        public SSOController(ITokenService tokenService, IOptions<JwtSettings> jwtSettings)
        {
            _tokenService = tokenService;
            _jwtSettings = jwtSettings.Value;
        }

        [HttpGet("initiate")]
        public IActionResult InitiateSSO([FromQuery] string returnUrl)
        {
            if (!IsValidRedirectUri(returnUrl))
            {
                return BadRequest(new { error = "Invalid redirect URI." });
            }

            var ssoUrl = CreateSSORedirectUrl(returnUrl);
            return Redirect(ssoUrl);
        }

        [HttpPost("callback")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SSOCallback([FromBody] SSOCallbackRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!IsValidSSOResponse(request.Token))
            {
                return Unauthorized(new { error = "Invalid or expired token." });
            }

            var user = GetUserFromSSOToken(request.Token);
            if (user == null)
            {
                return Unauthorized(new { error = "User not found." });
            }

            var (token, expiresAt) = await _tokenService.GenerateTokenAsync(user);

            return Redirect(QueryHelpers.AddQueryString(request.ReturnUrl, "token", token));
        }

        private bool IsValidRedirectUri(string? redirectUri)
        {
            // Validate against pre-configured URIs
            return !string.IsNullOrEmpty(redirectUri) && _jwtSettings.ValidRedirectUris.Contains(redirectUri);
        }

        private string CreateSSORedirectUrl(string returnUrl)
        {
            // Construct an SSO URL specific to the identity provider
            return $"{_jwtSettings.SSOProviderUrl}?returnUrl={Uri.EscapeDataString(returnUrl)}";
        }

        private bool IsValidSSOResponse(string token)
        {
            // Validate the token from the SSO provider (for time constraints, logic is assumed)
            return !string.IsNullOrEmpty(token); // Placeholder for actual validation
        }

        private ClaimsPrincipal? GetUserFromSSOToken(string token)
        {
            // Extract user info from SSO token
            return new ClaimsPrincipal(); // Placeholder for actual extraction
        }
    }

    public class SSOCallbackRequest
    {
        public string Token { get; set; }
        public string ReturnUrl { get; set; }
    }
}
```