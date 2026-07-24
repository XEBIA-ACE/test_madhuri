```csharp
using AuthenticationService.Controllers;
using AuthenticationService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Xunit;

namespace AuthenticationService.Tests
{
    public class SSOControllerTests
    {
        private readonly SSOController _ssoController;
        private readonly ITokenService _tokenService;

        public SSOControllerTests()
        {
            var jwtSettings = Options.Create(new JwtSettings
            {
                SSOProviderUrl = "https://sso.example.com",
                ValidRedirectUris = new List<string> { "https://app.example.com/return" }
            });

            _tokenService = new MockTokenService();
            _ssoController = new SSOController(_tokenService, jwtSettings);
        }

        [Fact]
        public async Task InitiateSSO_ReturnsRedirectForValidUri()
        {
            // Arrange
            var returnUrl = "https://app.example.com/return";

            // Act
            var result = _ssoController.InitiateSSO(returnUrl);

            // Assert
            var redirectResult = Assert.IsType<RedirectResult>(result);
            Assert.Contains("https://sso.example.com", redirectResult.Url);
        }

        [Fact]
        public void InitiateSSO_ReturnsBadRequestForInvalidUri()
        {
            // Arrange
            var returnUrl = "https://invalid.example.com";

            // Act
            var result = _ssoController.InitiateSSO(returnUrl);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Invalid redirect URI", badRequest.Value.ToString());
        }

        private class MockTokenService : ITokenService
        {
            // Mock Implementation
        }
    }
}
```