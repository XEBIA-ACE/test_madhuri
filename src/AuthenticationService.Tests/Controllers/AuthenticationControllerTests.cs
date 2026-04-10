using AuthenticationService.Controllers;
using AuthenticationService.Models.Entities;
using AuthenticationService.Models.Requests;
using AuthenticationService.Models.Responses;
using AuthenticationService.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AuthenticationService.Tests.Controllers;

public class AuthenticationControllerTests
{
    private readonly Mock<IUserCredentialValidator> _credentialValidatorMock;
    private readonly Mock<ITokenManager> _tokenManagerMock;
    private readonly Mock<ILogger<AuthenticationController>> _loggerMock;
    private readonly AuthenticationController _sut;

    public AuthenticationControllerTests()
    {
        _credentialValidatorMock = new Mock<IUserCredentialValidator>();
        _tokenManagerMock = new Mock<ITokenManager>();
        _loggerMock = new Mock<ILogger<AuthenticationController>>();

        _sut = new AuthenticationController(
            _credentialValidatorMock.Object,
            _tokenManagerMock.Object,
            _loggerMock.Object);
    }

    #region Login Tests

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnOkWithToken()
    {
        // Arrange
        var request = new LoginRequest { Username = "testuser", Password = "testpassword" };
        var tokenInfo = new TokenInfo
        {
            Token = "test-token",
            Username = "testuser",
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        _credentialValidatorMock
            .Setup(x => x.ValidateAsync(request.Username, request.Password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CredentialValidationResult.Success(request.Username));

        _tokenManagerMock
            .Setup(x => x.IssueTokenAsync(request.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tokenInfo);

        // Act
        var result = await _sut.Login(request, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<LoginResponse>().Subject;
        response.Token.Should().Be("test-token");
        response.ExpiresAt.Should().Be(tokenInfo.ExpiresAt);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new LoginRequest { Username = "testuser", Password = "wrongpassword" };

        _credentialValidatorMock
            .Setup(x => x.ValidateAsync(request.Username, request.Password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CredentialValidationResult.Failure("Invalid username or password"));

        // Act
        var result = await _sut.Login(request, CancellationToken.None);

        // Assert
        var unauthorizedResult = result.Should().BeOfType<UnauthorizedObjectResult>().Subject;
        var response = unauthorizedResult.Value.Should().BeOfType<ErrorResponse>().Subject;
        response.ErrorCode.Should().Be("AUTH_INVALID_CREDENTIALS");
        response.Message.Should().Be("Invalid username or password");
    }

    #endregion

    #region Validate Tests

    [Fact]
    public async Task Validate_WithValidToken_ShouldReturnOkWithValidTrue()
    {
        // Arrange
        var request = new ValidateTokenRequest { Token = "valid-token" };
        var tokenInfo = new TokenInfo
        {
            Token = "valid-token",
            Username = "testuser",
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        _tokenManagerMock
            .Setup(x => x.ValidateTokenAsync(request.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tokenInfo);

        // Act
        var result = await _sut.Validate(request, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ValidateTokenResponse>().Subject;
        response.Valid.Should().BeTrue();
        response.ExpiresAt.Should().Be(tokenInfo.ExpiresAt);
    }

    [Fact]
    public async Task Validate_WithInvalidToken_ShouldReturnOkWithValidFalse()
    {
        // Arrange
        var request = new ValidateTokenRequest { Token = "invalid-token" };

        _tokenManagerMock
            .Setup(x => x.ValidateTokenAsync(request.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TokenInfo?)null);

        // Act
        var result = await _sut.Validate(request, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<ValidateTokenResponse>().Subject;
        response.Valid.Should().BeFalse();
        response.ExpiresAt.Should().BeNull();
    }

    #endregion

    #region Logout Tests

    [Fact]
    public async Task Logout_WithValidToken_ShouldReturnOkWithSuccess()
    {
        // Arrange
        var request = new LogoutRequest { Token = "valid-token" };

        _tokenManagerMock
            .Setup(x => x.InvalidateTokenAsync(request.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.Logout(request, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<LogoutResponse>().Subject;
        response.Success.Should().BeTrue();
        response.Message.Should().Be("Logout successful");
    }

    [Fact]
    public async Task Logout_WithUnknownToken_ShouldStillReturnSuccess()
    {
        // Arrange - Token not found, but we still return success to prevent enumeration
        var request = new LogoutRequest { Token = "unknown-token" };

        _tokenManagerMock
            .Setup(x => x.InvalidateTokenAsync(request.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.Logout(request, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<LogoutResponse>().Subject;
        response.Success.Should().BeTrue();
    }

    #endregion

    #region Health Tests

    [Fact]
    public void Health_ShouldReturnOk()
    {
        // Act
        var result = _sut.Health();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    #endregion
}
