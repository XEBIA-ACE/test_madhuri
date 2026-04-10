using AuthenticationService.Models.Entities;
using AuthenticationService.Services;
using AuthenticationService.Stores;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AuthenticationService.Tests.Services;

public class UserCredentialValidatorTests
{
    private readonly Mock<IUserStore> _userStoreMock;
    private readonly Mock<ILogger<UserCredentialValidator>> _loggerMock;
    private readonly UserCredentialValidator _sut;

    public UserCredentialValidatorTests()
    {
        _userStoreMock = new Mock<IUserStore>();
        _loggerMock = new Mock<ILogger<UserCredentialValidator>>();
        _sut = new UserCredentialValidator(_userStoreMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ValidateAsync_WithValidCredentials_ShouldReturnSuccess()
    {
        // Arrange
        var username = "testuser";
        var password = "testpassword";
        var passwordHash = InMemoryUserStore.HashPassword(password);

        var user = new UserCredentials
        {
            UserId = "user-001",
            Username = username,
            PasswordHash = passwordHash,
            IsActive = true
        };

        _userStoreMock
            .Setup(x => x.GetByUsernameAsync(username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.ValidateAsync(username, password);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Username.Should().Be(username);
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public async Task ValidateAsync_WithInvalidPassword_ShouldReturnFailure()
    {
        // Arrange
        var username = "testuser";
        var correctPassword = "correctpassword";
        var wrongPassword = "wrongpassword";
        var passwordHash = InMemoryUserStore.HashPassword(correctPassword);

        var user = new UserCredentials
        {
            UserId = "user-001",
            Username = username,
            PasswordHash = passwordHash,
            IsActive = true
        };

        _userStoreMock
            .Setup(x => x.GetByUsernameAsync(username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.ValidateAsync(username, wrongPassword);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("Invalid username or password");
    }

    [Fact]
    public async Task ValidateAsync_WithNonExistentUser_ShouldReturnFailure()
    {
        // Arrange
        var username = "nonexistent";
        var password = "anypassword";

        _userStoreMock
            .Setup(x => x.GetByUsernameAsync(username, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserCredentials?)null);

        // Act
        var result = await _sut.ValidateAsync(username, password);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("Invalid username or password");
    }

    [Fact]
    public async Task ValidateAsync_WithInactiveUser_ShouldReturnFailure()
    {
        // Arrange
        var username = "inactiveuser";
        var password = "testpassword";
        var passwordHash = InMemoryUserStore.HashPassword(password);

        var user = new UserCredentials
        {
            UserId = "user-001",
            Username = username,
            PasswordHash = passwordHash,
            IsActive = false // Inactive
        };

        _userStoreMock
            .Setup(x => x.GetByUsernameAsync(username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.ValidateAsync(username, password);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("Account is inactive");
    }

    [Theory]
    [InlineData("", "password")]
    [InlineData("   ", "password")]
    [InlineData(null, "password")]
    public async Task ValidateAsync_WithEmptyUsername_ShouldReturnFailure(string? username, string password)
    {
        // Act
        var result = await _sut.ValidateAsync(username!, password);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("Username is required");
    }

    [Theory]
    [InlineData("username", "")]
    [InlineData("username", "   ")]
    [InlineData("username", null)]
    public async Task ValidateAsync_WithEmptyPassword_ShouldReturnFailure(string username, string? password)
    {
        // Act
        var result = await _sut.ValidateAsync(username, password!);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("Password is required");
    }
}
