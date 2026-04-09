using AuthenticationService.Core.Configuration;
using AuthenticationService.Core.Entities;
using AuthenticationService.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace AuthenticationService.Tests.Services;

public class JwtTokenServiceTests
{
    private readonly JwtTokenService _sut;
    private readonly JwtSettings _jwtSettings;

    public JwtTokenServiceTests()
    {
        _jwtSettings = new JwtSettings
        {
            SecretKey = "TestSecretKeyThatIsAtLeast32CharactersLongForTesting!",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            AccessTokenExpirationMinutes = 60,
            RefreshTokenExpirationDays = 7
        };

        _sut = new JwtTokenService(Options.Create(_jwtSettings));
    }

    [Fact]
    public void GenerateAccessToken_ShouldReturnValidJwt()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com"
        };

        // Act
        var token = _sut.GenerateAccessToken(user);

        // Assert
        token.Should().NotBeNullOrEmpty();
        token.Split('.').Should().HaveCount(3); // JWT has 3 parts
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnNonEmptyString()
    {
        // Act
        var refreshToken = _sut.GenerateRefreshToken();

        // Assert
        refreshToken.Should().NotBeNullOrEmpty();
        refreshToken.Length.Should().BeGreaterThan(20);
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnUniqueTokens()
    {
        // Act
        var token1 = _sut.GenerateRefreshToken();
        var token2 = _sut.GenerateRefreshToken();

        // Assert
        token1.Should().NotBe(token2);
    }

    [Fact]
    public void ValidateAccessToken_WithValidToken_ShouldReturnUserInfo()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "test@example.com";
        var user = new User
        {
            Id = userId,
            Email = email
        };

        var token = _sut.GenerateAccessToken(user);

        // Act
        var (isValid, returnedUserId, returnedEmail) = _sut.ValidateAccessToken(token);

        // Assert
        isValid.Should().BeTrue();
        returnedUserId.Should().Be(userId);
        returnedEmail.Should().Be(email);
    }

    [Fact]
    public void ValidateAccessToken_WithInvalidToken_ShouldReturnInvalid()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var (isValid, userId, email) = _sut.ValidateAccessToken(invalidToken);

        // Assert
        isValid.Should().BeFalse();
        userId.Should().BeNull();
        email.Should().BeNull();
    }

    [Fact]
    public void ValidateAccessToken_WithTamperedToken_ShouldReturnInvalid()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com"
        };

        var token = _sut.GenerateAccessToken(user);
        var tamperedToken = token + "tampered";

        // Act
        var (isValid, userId, email) = _sut.ValidateAccessToken(tamperedToken);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void GetAccessTokenExpiration_ShouldReturnFutureDate()
    {
        // Act
        var expiration = _sut.GetAccessTokenExpiration();

        // Assert
        expiration.Should().BeAfter(DateTime.UtcNow);
        expiration.Should().BeBefore(DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes + 1));
    }
}
