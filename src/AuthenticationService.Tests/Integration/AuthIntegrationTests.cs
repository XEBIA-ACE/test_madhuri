using System.Net;
using System.Net.Http.Json;
using AuthenticationService.Core.DTOs;
using AuthenticationService.Infrastructure.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AuthenticationService.Tests.Integration;

public class AuthIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AuthIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove existing DbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AuthDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add in-memory database for testing
                services.AddDbContext<AuthDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid());
                });
            });
        });

        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = $"test_{Guid.NewGuid()}@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<RegisterResponse>();
        result.Should().NotBeNull();
        result!.Email.Should().Be(request.Email.ToLower());
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldReturnConflict()
    {
        // Arrange
        var email = $"duplicate_{Guid.NewGuid()}@example.com";
        var request = new RegisterRequest
        {
            Email = email,
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        // First registration
        await _client.PostAsJsonAsync("/api/auth/register", request);

        // Act - Second registration with same email
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnTokens()
    {
        // Arrange
        var email = $"login_{Guid.NewGuid()}@example.com";
        var password = "Password123!";

        // Register first
        var registerRequest = new RegisterRequest
        {
            Email = email,
            Password = password,
            ConfirmPassword = password
        };
        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

        // Act
        var loginRequest = new LoginRequest
        {
            Email = email,
            Password = password
        };
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.TokenType.Should().Be("Bearer");
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "WrongPassword!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_ShouldReturnOk()
    {
        // Arrange
        var request = new LogoutRequest { Token = "any_token" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/logout", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<LogoutResponse>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateToken_WithInvalidToken_ShouldReturnInvalid()
    {
        // Arrange
        var request = new TokenValidationRequest { Token = "invalid_token" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/token/validate", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<TokenValidationResponse>();
        result.Should().NotBeNull();
        result!.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Health_ShouldReturnHealthy()
    {
        // Act
        var response = await _client.GetAsync("/api/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<HealthResponse>();
        result.Should().NotBeNull();
        result!.Status.Should().Be("Healthy");
        result.ServiceId.Should().Be("AUTH-1");
        result.ServiceName.Should().Be("Authentication Service");
    }

    [Fact]
    public async Task HealthLive_ShouldReturnAlive()
    {
        // Act
        var response = await _client.GetAsync("/api/health/live");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task HealthReady_ShouldReturnReady()
    {
        // Act
        var response = await _client.GetAsync("/api/health/ready");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
