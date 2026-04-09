using AuthenticationService.Core.Configuration;
using AuthenticationService.Core.DTOs;
using AuthenticationService.Core.Entities;
using AuthenticationService.Core.Exceptions;
using AuthenticationService.Core.Interfaces;
using AuthenticationService.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace AuthenticationService.Tests.Services;

public class AuthenticationServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<ISessionRepository> _sessionRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly IOptions<JwtSettings> _jwtSettings;
    private readonly AuthenticationServiceImpl _sut;

    public AuthenticationServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _sessionRepositoryMock = new Mock<ISessionRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _tokenServiceMock = new Mock<ITokenService>();
        _jwtSettings = Options.Create(new JwtSettings
        {
            SecretKey = "TestSecretKeyThatIsAtLeast32CharactersLong!",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            AccessTokenExpirationMinutes = 60,
            RefreshTokenExpirationDays = 7
        });

        _sut = new AuthenticationServiceImpl(
            _userRepositoryMock.Object,
            _sessionRepositoryMock.Object,
            _passwordHasherMock.Object,
            _tokenServiceMock.Object,
            _jwtSettings);
    }

    #region Register Tests

    [Fact]
    public async Task RegisterAsync_WithValidRequest_ShouldReturnSuccess()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        var createdUser = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email.ToLower(),
            CreatedAt = DateTime.UtcNow
        };

        _userRepositoryMock.Setup(x => x.ExistsAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _passwordHasherMock.Setup(x => x.HashPassword(request.Password))
            .Returns("hashedPassword");
        _userRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdUser);

        // Act
        var result = await _sut.RegisterAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(createdUser.Id);
        result.Email.Should().Be(createdUser.Email);
        result.Message.Should().Be("Registration successful");
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ShouldThrowUserAlreadyExistsException()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "existing@example.com",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        _userRepositoryMock.Setup(x => x.ExistsAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<UserAlreadyExistsException>(() => _sut.RegisterAsync(request));
    }

    #endregion

    #region Login Tests

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnTokens()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = "hashedPassword",
            IsActive = true
        };

        var accessToken = "access_token";
        var refreshToken = "refresh_token";
        var expiresAt = DateTime.UtcNow.AddHours(1);

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(x => x.VerifyPassword(request.Password, user.PasswordHash))
            .Returns(true);
        _tokenServiceMock.Setup(x => x.GenerateAccessToken(user))
            .Returns(accessToken);
        _tokenServiceMock.Setup(x => x.GenerateRefreshToken())
            .Returns(refreshToken);
        _tokenServiceMock.Setup(x => x.GetAccessTokenExpiration())
            .Returns(expiresAt);
        _sessionRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Session>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Session s, CancellationToken _) => s);

        // Act
        var result = await _sut.LoginAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(user.Id);
        result.Email.Should().Be(user.Email);
        result.AccessToken.Should().Be(accessToken);
        result.RefreshToken.Should().Be(refreshToken);
        result.TokenType.Should().Be("Bearer");
    }

    [Fact]
    public async Task LoginAsync_WithInvalidEmail_ShouldThrowInvalidCredentialsException()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "Password123!"
        };

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidCredentialsException>(() => _sut.LoginAsync(request));
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ShouldThrowInvalidCredentialsException()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "WrongPassword!"
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = "hashedPassword",
            IsActive = true
        };

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(x => x.VerifyPassword(request.Password, user.PasswordHash))
            .Returns(false);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidCredentialsException>(() => _sut.LoginAsync(request));
    }

    [Fact]
    public async Task LoginAsync_WithInactiveUser_ShouldThrowInvalidCredentialsException()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "inactive@example.com",
            Password = "Password123!"
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = "hashedPassword",
            IsActive = false
        };

        _userRepositoryMock.Setup(x => x.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidCredentialsException>(() => _sut.LoginAsync(request));
    }

    #endregion

    #region Logout Tests

    [Fact]
    public async Task LogoutAsync_WithValidToken_ShouldReturnSuccess()
    {
        // Arrange
        var request = new LogoutRequest { Token = "valid_token" };
        var session = new Session
        {
            Id = Guid.NewGuid(),
            Token = request.Token,
            RevokedAt = null
        };

        _sessionRepositoryMock.Setup(x => x.GetByTokenAsync(request.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        _sessionRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Session>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Session s, CancellationToken _) => s);

        // Act
        var result = await _sut.LogoutAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Be("Logout successful");
    }

    [Fact]
    public async Task LogoutAsync_WithInvalidToken_ShouldStillReturnSuccess()
    {
        // Arrange
        var request = new LogoutRequest { Token = "invalid_token" };

        _sessionRepositoryMock.Setup(x => x.GetByTokenAsync(request.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Session?)null);

        // Act
        var result = await _sut.LogoutAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
    }

    #endregion

    #region Token Validation Tests

    [Fact]
    public async Task ValidateTokenAsync_WithValidToken_ShouldReturnValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "test@example.com";
        var request = new TokenValidationRequest { Token = "valid_token" };

        var user = new User
        {
            Id = userId,
            Email = email,
            IsActive = true
        };

        var session = new Session
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = request.Token,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            RevokedAt = null
        };

        _tokenServiceMock.Setup(x => x.ValidateAccessToken(request.Token))
            .Returns((true, userId, email));
        _sessionRepositoryMock.Setup(x => x.GetByTokenAsync(request.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);
        _userRepositoryMock.Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.ValidateTokenAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.UserId.Should().Be(userId);
        result.Email.Should().Be(email);
    }

    [Fact]
    public async Task ValidateTokenAsync_WithInvalidToken_ShouldReturnInvalid()
    {
        // Arrange
        var request = new TokenValidationRequest { Token = "invalid_token" };

        _tokenServiceMock.Setup(x => x.ValidateAccessToken(request.Token))
            .Returns((false, null, null));

        // Act
        var result = await _sut.ValidateTokenAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Message.Should().Be("Invalid or expired token");
    }

    [Fact]
    public async Task ValidateTokenAsync_WithRevokedSession_ShouldReturnInvalid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new TokenValidationRequest { Token = "revoked_token" };

        var session = new Session
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = request.Token,
            RevokedAt = DateTime.UtcNow.AddMinutes(-5)
        };

        _tokenServiceMock.Setup(x => x.ValidateAccessToken(request.Token))
            .Returns((true, userId, "test@example.com"));
        _sessionRepositoryMock.Setup(x => x.GetByTokenAsync(request.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // Act
        var result = await _sut.ValidateTokenAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.Message.Should().Be("Token has been revoked");
    }

    #endregion
}
