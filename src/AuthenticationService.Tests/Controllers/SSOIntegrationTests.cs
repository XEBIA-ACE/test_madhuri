```csharp
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AuthenticationService.Models.DTOs;
using Xunit;

namespace AuthenticationService.Tests.Controllers
{
    public class SSOIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public SSOIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task SSO_Endpoint_Should_ReturnRedirect_For_ValidRequest()
        {
            // Arrange
            var ssoRequest = new SSORequest
            {
                // Assume necessary SSORequest properties and values
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/authentication/sso", ssoRequest);

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Contains("identityprovider.com", response.Headers.Location.Host); // Check if redirected to the identity provider
        }

        [Fact]
        public async Task SSO_Endpoint_Should_Reject_InvalidRedirectUri()
        {
            // Arrange
            var ssoRequest = new SSORequest
            {
                RedirectUri = "http://malicious.com/callback" // Invalid redirect URI not pre-registered
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/authentication/sso", ssoRequest);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            Assert.Equal("invalid_redirect_uri", errorResponse.ErrorCode);
        }
    }
}
```