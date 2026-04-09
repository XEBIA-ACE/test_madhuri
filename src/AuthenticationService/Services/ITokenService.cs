using AuthenticationService.Models;

namespace AuthenticationService.Services;

/// <summary>
/// Interface for JWT token operations.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates a JWT token for the specified user.
    /// </summary>
    string GenerateToken(User user);

    /// <summary>
    /// Validates a JWT token and returns the claims if valid.
    /// </summary>
    TokenValidationResult ValidateToken(string token);
}

/// <summary>
/// Result of token validation.
/// </summary>
public class TokenValidationResult
{
    public bool IsValid { get; set; }
    public Guid? UserId { get; set; }
    public string? Email { get; set; }
    public string? ErrorMessage { get; set; }
}
