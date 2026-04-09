using AuthenticationService.Core.Entities;

namespace AuthenticationService.Core.Interfaces;

/// <summary>
/// Interface for JWT token operations.
/// </summary>
public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    (bool IsValid, Guid? UserId, string? Email) ValidateAccessToken(string token);
    DateTime GetAccessTokenExpiration();
}
