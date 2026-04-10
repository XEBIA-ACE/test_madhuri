using AuthenticationService.Models.Entities;
using AuthenticationService.Stores;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AuthenticationService.Tests.Stores;

public class InMemoryTokenStoreTests
{
    private readonly Mock<ILogger<InMemoryTokenStore>> _loggerMock;
    private readonly InMemoryTokenStore _sut;

    public InMemoryTokenStoreTests()
    {
        _loggerMock = new Mock<ILogger<InMemoryTokenStore>>();
        _sut = new InMemoryTokenStore(_loggerMock.Object);
    }

    [Fact]
    public async Task StoreAsync_ShouldStoreToken()
    {
        // Arrange
        var tokenInfo = CreateTokenInfo("test-token", "testuser");

        // Act
        await _sut.StoreAsync(tokenInfo);
        var result = await _sut.GetAsync("test-token");

        // Assert
        result.Should().NotBeNull();
        result!.Token.Should().Be("test-token");
        result.Username.Should().Be("testuser");
    }

    [Fact]
    public async Task GetAsync_WithNonExistentToken_ShouldReturnNull()
    {
        // Act
        var result = await _sut.GetAsync("non-existent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task InvalidateAsync_WithExistingToken_ShouldInvalidateAndReturnTrue()
    {
        // Arrange
        var tokenInfo = CreateTokenInfo("test-token", "testuser");
        await _sut.StoreAsync(tokenInfo);

        // Act
        var result = await _sut.InvalidateAsync("test-token");
        var storedToken = await _sut.GetAsync("test-token");

        // Assert
        result.Should().BeTrue();
        storedToken.Should().NotBeNull();
        storedToken!.IsInvalidated.Should().BeTrue();
        storedToken.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task InvalidateAsync_WithNonExistentToken_ShouldReturnFalse()
    {
        // Act
        var result = await _sut.InvalidateAsync("non-existent");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task InvalidateAllForUserAsync_ShouldInvalidateAllUserTokens()
    {
        // Arrange
        await _sut.StoreAsync(CreateTokenInfo("token1", "user1"));
        await _sut.StoreAsync(CreateTokenInfo("token2", "user1"));
        await _sut.StoreAsync(CreateTokenInfo("token3", "user2"));

        // Act
        var count = await _sut.InvalidateAllForUserAsync("user1");

        // Assert
        count.Should().Be(2);

        var token1 = await _sut.GetAsync("token1");
        var token2 = await _sut.GetAsync("token2");
        var token3 = await _sut.GetAsync("token3");

        token1!.IsInvalidated.Should().BeTrue();
        token2!.IsInvalidated.Should().BeTrue();
        token3!.IsInvalidated.Should().BeFalse();
    }

    [Fact]
    public async Task CleanupExpiredAsync_ShouldRemoveExpiredTokens()
    {
        // Arrange
        var expiredToken = new TokenInfo
        {
            Token = "expired-token",
            Username = "testuser",
            IssuedAt = DateTime.UtcNow.AddHours(-2),
            ExpiresAt = DateTime.UtcNow.AddHours(-1) // Expired
        };

        var validToken = CreateTokenInfo("valid-token", "testuser");

        await _sut.StoreAsync(expiredToken);
        await _sut.StoreAsync(validToken);

        // Act
        var count = await _sut.CleanupExpiredAsync();

        // Assert
        count.Should().Be(1);

        var expired = await _sut.GetAsync("expired-token");
        var valid = await _sut.GetAsync("valid-token");

        expired.Should().BeNull();
        valid.Should().NotBeNull();
    }

    private static TokenInfo CreateTokenInfo(string token, string username)
    {
        return new TokenInfo
        {
            Token = token,
            Username = username,
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };
    }
}
