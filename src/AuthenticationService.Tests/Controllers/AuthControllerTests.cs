using AuthenticationService.Api.Controllers;
using AuthenticationService.Core.DTOs;
using AuthenticationService.Core.Exceptions;
using AuthenticationService.Core.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AuthenticationService.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthenticationService> _authServiceMock;
    private readonly Mock<ILogger<AuthController>> _loggerMock;
    private readonly AuthController _sut;

    public AuthControllerTests()
    {
        _authServiceMock = new Mock<IAuthenticationService>();
        _loggerMock = new Mock<ILogger<AuthController>>();
        _sut = new AuthController(_authServiceMock.Object, _loggerMock.Object);
    }

    #region Register Tests

    [Fact]
    public async Task Register_WithValidRequest_ShouldReturnCreated()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        var response = new RegisterResponse
        {
            UserId = Guid.NewGuid(),
            Email = request.Email,
            Message = "Registration successful",
            CreatedAt = DateTime.UtcNow
        };

        _authServiceMock.Setup(x => x.RegisterAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _sut.Register(request, CancellationToken.None);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var returnedResponse = createdResult.Value.Should().BeOfType<RegisterResponse>().Subject;
        returnedResponse.Email.Should().Be(request.Email);
    }

    [Fact]
    public async Task Register_WithExistingEmail_ShouldReturnConflict()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "existing@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        _authServiceMock.Setup(x => x.RegisterAsync(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UserAlreadyExistsException(request.Email));

        // Act
        var result = await _sut.Register(request, CancellationToken.None);

        // Assert
        var conflictResult = result.Should().BeOfType<ConflictObjectResult>().Subject;
        conflictResult.StatusCode.Should().Be(409);
    }

    #endregion

    #region Login Tests

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnOk()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        var response = new LoginResponse
        {
            UserId = Guid.NewGuid(),
            Email = request.Email,
            AccessToken = "access_token",
            RefreshToken = "refresh_token",
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            TokenType = "Bearer"
        };

        _authServiceMock.Setup(x => x.LoginAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _sut.Login(request, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedResponse = okResult.Value.Should().BeOfType<LoginResponse>().Subject;
        returnedResponse.AccessToken.Should().Be("access_token");
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "WrongPassword!"
        };

        _authServiceMock.Setup(x => x.LoginAsync(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidCredentialsException());

        // Act
        var result = await _sut.Login(request, CancellationToken.None);

        // Assert
        var unauthorizedResult = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        unauthorizedResult.StatusCode.Should().Be(401);
    }

    #endregion

    #region Logout Tests

    [Fact]
    public async Task Logout_ShouldReturnOk()
    {
        // Arrange
        var request = new LogoutRequest { Token = "valid_token" };
        var response = new LogoutResponse
        {
            Success = true,
            Message = "Logout successful"
        };

        _authServiceMock.Setup(x => x.LogoutAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _sut.Logout(request, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedResponse = okResult.Value.Should().BeOfType<LogoutResponse>().Subject;
        returnedResponse.Success.Should().BeTrue();
    }

    #endregion

    #region Token Validation Tests

    [Fact]
    public async Task ValidateToken_WithValidToken_ShouldReturnOk()
    {
        // Arrange
        var request = new TokenValidationRequest { Token = "valid_token" };
        var response = new TokenValidationResponse
        {
            IsValid = true,
            UserId = Guid.NewGuid(),
            Email = "test@example.com",
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            Message = "Token is valid"
        };

        _authServiceMock.Setup(x => x.ValidateTokenAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _sut.ValidateToken(request, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedResponse = okResult.Value.Should().BeOfType<TokenValidationResponse>().Subject;
        returnedResponse.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateToken_WithInvalidToken_ShouldReturnOkWithInvalidStatus()
    {
        // Arrange
        var request = new TokenValidationRequest { Token = "invalid_token" };
        var response = new TokenValidationResponse
        {
            IsValid = false,
            Message = "Invalid or expired token"
        };

        _authServiceMock.Setup(x => x.ValidateTokenAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _sut.ValidateToken(request, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedResponse = okResult.Value.Should().BeOfType<TokenValidationResponse>().Subject;
        returnedResponse.IsValid.Should().BeFalse();
    }

    #endregion
}
