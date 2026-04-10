using AuthenticationService.Configuration;
using AuthenticationService.Models.Entities;
using AuthenticationService.Services;
using AuthenticationService.Stores;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace AuthenticationService.Tests.Services;

public class TokenManagerTests
{
    private readonly Mock<ITokenStore> _tokenStoreMock;
    private readonly Mock<ILogger<TokenManager>> _loggerMock;
    private readonly IOptions<TokenSettings> _settings;
    private readonly TokenManager _sut;

    public TokenManagerTests()
    {
        _tokenStoreMock = new Mock<ITokenStore>();
        _loggerMock = new Mock<ILogger<TokenManager>>();
        _settings = Options.Create(new TokenSettings
        {
            TokenExpirationMinutes = 60
        });

        _sut = new TokenManager(_tokenStoreMock.Object, _settings, _loggerMock.Object);
    }

    [Fact]
    public async Task IssueTokenAsync_ShouldReturnValidToken()
    {
        // Arrange
        var username = "testuser";
        _tokenStoreMock
            .Setup(x => x.StoreAsync(It.IsAny<TokenInfo>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.IssueTokenAsync(username);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().NotBeNullOrEmpty();
        result.Username.Should().Be(username);
        result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
        result.IsInvalidated.Should().BeFalse();
        result.IsValid.Should().BeTrue();

        _tokenStoreMock.Verify(
            x => x.StoreAsync(It.Is<TokenInfo>(t => t.Username == username), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task IssueTokenAsync_ShouldSetCorrectExpiration()
    {
        // Arrange
        var username = "testuser";
        var expectedExpirationMinutes = 60;

        // Act
        var result = await _sut.IssueTokenAsync(username);

        // Assert
        var expectedExpiration = DateTime.UtcNow.AddMinutes(expectedExpirationMinutes);
        result.ExpiresAt.Should().BeCloseTo(expectedExpiration, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task ValidateTokenAsync_WithValidToken_ShouldReturnTokenInfo()
    {
        // Arrange
        var token = "valid-token";
        var tokenInfo = new TokenInfo
        {
            Token = token,
            Username = "testuser",
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            IsInvalidated = false
        };

        _tokenStoreMock
            .Setup(x => x.GetAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tokenInfo);

        // Act
        var result = await _sut.ValidateTokenAsync(token);

        // Assert
        result.Should().NotBeNull();
        result!.Token.Should().Be(token);
        result.Username.Should().Be("testuser");
    }

    [Fact]
    public async Task ValidateTokenAsync_WithExpiredToken_ShouldReturnNull()
    {
        // Arrange
        var token = "expired-token";
        var tokenInfo = new TokenInfo
        {
            Token = token,
            Username = "testuser",
            IssuedAt = DateTime.UtcNow.AddHours(-2),
            ExpiresAt = DateTime.UtcNow.AddHours(-1), // Expired
            IsInvalidated = false
        };

        _tokenStoreMock
            .Setup(x => x.GetAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tokenInfo);

        // Act
        var result = await _sut.ValidateTokenAsync(token);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ValidateTokenAsync_WithInvalidatedToken_ShouldReturnNull()
    {
        // Arrange
        var token = "invalidated-token";
        var tokenInfo = new TokenInfo
        {
            Token = token,
            Username = "testuser",
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            IsInvalidated = true // Invalidated
        };

        _tokenStoreMock
            .Setup(x => x.GetAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tokenInfo);

        // Act
        var result = await _sut.ValidateTokenAsync(token);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ValidateTokenAsync_WithNonExistentToken_ShouldReturnNull()
    {
        // Arrange
        var token = "non-existent-token";
        _tokenStoreMock
            .Setup(x => x.GetAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TokenInfo?)null);

        // Act
        var result = await _sut.ValidateTokenAsync(token);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task InvalidateTokenAsync_WithExistingToken_ShouldReturnTrue()
    {
        // Arrange
        var token = "existing-token";
        _tokenStoreMock
            .Setup(x => x.InvalidateAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.InvalidateTokenAsync(token);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task InvalidateTokenAsync_WithNonExistentToken_ShouldReturnFalse()
    {
        // Arrange
        var token = "non-existent-token";
        _tokenStoreMock
            .Setup(x => x.InvalidateAsync(token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.InvalidateTokenAsync(token);

        // Assert
        result.Should().BeFalse();
    }
}
