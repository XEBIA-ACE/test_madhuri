using AuthenticationService.Models.Entities;

namespace AuthenticationService.Services;

/// <summary>
/// Interface for token lifecycle management.
/// </summary>
public interface ITokenManager
{
    /// <summary>
    /// Issues a new authentication token for a user.
    /// </summary>
    /// <param name="username">The username to issue the token for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The issued token information.</returns>
    Task<TokenInfo> IssueTokenAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a token and returns its information if valid.
    /// </summary>
    /// <param name="token">The token to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Token information if valid, null otherwise.</returns>
    Task<TokenInfo?> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates a token (logout).
    /// </summary>
    /// <param name="token">The token to invalidate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if token was invalidated, false if not found.</returns>
    Task<bool> InvalidateTokenAsync(string token, CancellationToken cancellationToken = default);
}
