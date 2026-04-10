using AuthenticationService.Stores;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AuthenticationService.Tests.Stores;

public class InMemoryUserStoreTests
{
    private readonly Mock<ILogger<InMemoryUserStore>> _loggerMock;
    private readonly InMemoryUserStore _sut;

    public InMemoryUserStoreTests()
    {
        _loggerMock = new Mock<ILogger<InMemoryUserStore>>();
        _sut = new InMemoryUserStore(_loggerMock.Object);
    }

    [Fact]
    public async Task GetByUsernameAsync_WithExistingUser_ShouldReturnUser()
    {
        // Act - testuser is seeded by default
        var result = await _sut.GetByUsernameAsync("testuser");

        // Assert
        result.Should().NotBeNull();
        result!.Username.Should().Be("testuser");
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetByUsernameAsync_WithNonExistentUser_ShouldReturnNull()
    {
        // Act
        var result = await _sut.GetByUsernameAsync("nonexistent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByUsernameAsync_ShouldBeCaseInsensitive()
    {
        // Act
        var result1 = await _sut.GetByUsernameAsync("TESTUSER");
        var result2 = await _sut.GetByUsernameAsync("TestUser");
        var result3 = await _sut.GetByUsernameAsync("testuser");

        // Assert
        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        result3.Should().NotBeNull();
    }

    [Fact]
    public async Task ExistsAsync_WithExistingUser_ShouldReturnTrue()
    {
        // Act
        var result = await _sut.ExistsAsync("testuser");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistentUser_ShouldReturnFalse()
    {
        // Act
        var result = await _sut.ExistsAsync("nonexistent");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void HashPassword_ShouldProduceConsistentHash()
    {
        // Arrange
        var password = "testpassword";

        // Act
        var hash1 = InMemoryUserStore.HashPassword(password);
        var hash2 = InMemoryUserStore.HashPassword(password);

        // Assert
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void HashPassword_ShouldProduceDifferentHashesForDifferentPasswords()
    {
        // Arrange
        var password1 = "password1";
        var password2 = "password2";

        // Act
        var hash1 = InMemoryUserStore.HashPassword(password1);
        var hash2 = InMemoryUserStore.HashPassword(password2);

        // Assert
        hash1.Should().NotBe(hash2);
    }
}
