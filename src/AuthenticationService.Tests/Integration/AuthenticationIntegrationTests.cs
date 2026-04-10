using System.Net;
using System.Net.Http.Json;
using AuthenticationService.Models.Requests;
using AuthenticationService.Models.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace AuthenticationService.Tests.Integration;

public class AuthenticationIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AuthenticationIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task FullAuthenticationFlow_ShouldWork()
    {
        // Step 1: Login with valid credentials
        var loginRequest = new LoginRequest { Username = "testuser", Password = "testpassword" };
        var loginResponse = await _client.PostAsJsonAsync("/login", loginRequest);

        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        loginResult.Should().NotBeNull();
        loginResult!.Token.Should().NotBeNullOrEmpty();
        loginResult.ExpiresAt.Should().BeAfter(DateTime.UtcNow);

        // Step 2: Validate the token
        var validateRequest = new ValidateTokenRequest { Token = loginResult.Token };
        var validateResponse = await _client.PostAsJsonAsync("/validate", validateRequest);

        validateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var validateResult = await validateResponse.Content.ReadFromJsonAsync<ValidateTokenResponse>();
        validateResult.Should().NotBeNull();
        validateResult!.Valid.Should().BeTrue();
        validateResult.ExpiresAt.Should().Be(loginResult.ExpiresAt);

        // Step 3: Logout
        var logoutRequest = new LogoutRequest { Token = loginResult.Token };
        var logoutResponse = await _client.PostAsJsonAsync("/logout", logoutRequest);

        logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var logoutResult = await logoutResponse.Content.ReadFromJsonAsync<LogoutResponse>();
        logoutResult.Should().NotBeNull();
        logoutResult!.Success.Should().BeTrue();

        // Step 4: Validate the token again (should be invalid now)
        var validateAfterLogoutResponse = await _client.PostAsJsonAsync("/validate", validateRequest);

        validateAfterLogoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var validateAfterLogoutResult = await validateAfterLogoutResponse.Content.ReadFromJsonAsync<ValidateTokenResponse>();
        validateAfterLogoutResult.Should().NotBeNull();
        validateAfterLogoutResult!.Valid.Should().BeFalse();
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new LoginRequest { Username = "testuser", Password = "wrongpassword" };

        // Act
        var response = await _client.PostAsJsonAsync("/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var result = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        result.Should().NotBeNull();
        result!.ErrorCode.Should().Be("AUTH_INVALID_CREDENTIALS");
    }

    [Fact]
    public async Task Login_WithNonExistentUser_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new LoginRequest { Username = "nonexistent", Password = "anypassword" };

        // Act
        var response = await _client.PostAsJsonAsync("/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Validate_WithInvalidToken_ShouldReturnValidFalse()
    {
        // Arrange
        var request = new ValidateTokenRequest { Token = "invalid-token" };

        // Act
        var response = await _client.PostAsJsonAsync("/validate", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ValidateTokenResponse>();
        result.Should().NotBeNull();
        result!.Valid.Should().BeFalse();
    }

    [Fact]
    public async Task Health_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Metrics_ShouldReturnMetricsSnapshot()
    {
        // Act
        var response = await _client.GetAsync("/metrics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AuthenticationMetricsSnapshot>();
        result.Should().NotBeNull();
    }

    private class AuthenticationMetricsSnapshot
    {
        public long TotalLoginAttempts { get; set; }
        public long SuccessfulLogins { get; set; }
        public long FailedLogins { get; set; }
        public long TokenValidations { get; set; }
        public long ValidTokens { get; set; }
        public long InvalidTokens { get; set; }
        public long Logouts { get; set; }
        public DateTime SnapshotTime { get; set; }
    }
}
