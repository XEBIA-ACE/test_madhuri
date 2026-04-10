using AuthenticationService.Models.Entities;

namespace AuthenticationService.Services;

/// <summary>
/// Service interface for token operations.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates a new JWT token for the specified user.
    /// </summary>
    /// <param name="user">The user to generate a token for.</param>
    /// <returns>A tuple containing the token string and expiration time.</returns>
    Task<(string Token, DateTime ExpiresAt)> GenerateTokenAsync(User user);

    /// <summary>
    /// Validates a token and returns the validation result.
    /// </summary>
    /// <param name="token">The token to validate.</param>
    /// <returns>A tuple indicating validity, user ID, username, expiration, and error message if invalid.</returns>
    Task<(bool IsValid, Guid? UserId, string? Username, DateTime? ExpiresAt, string? ErrorMessage)> ValidateTokenAsync(string token);

    /// <summary>
    /// Invalidates a token (logout).
    /// </summary>
    /// <param name="token">The token to invalidate.</param>
    /// <returns>True if the token was successfully invalidated, false otherwise.</returns>
    Task<bool> InvalidateTokenAsync(string token);

    /// <summary>
    /// Extracts the token ID (jti claim) from a token without full validation.
    /// </summary>
    /// <param name="token">The token to extract from.</param>
    /// <returns>The token ID if found, null otherwise.</returns>
    string? ExtractTokenId(string token);
}
