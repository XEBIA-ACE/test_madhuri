using AuthenticationService.Models.Entities;

namespace AuthenticationService.Stores;

/// <summary>
/// Interface for token/session store operations.
/// </summary>
public interface ITokenStore
{
    /// <summary>
    /// Stores a new token.
    /// </summary>
    /// <param name="tokenInfo">The token information to store.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task StoreAsync(TokenInfo tokenInfo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets token information by token value.
    /// </summary>
    /// <param name="token">The token value to look up.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Token information if found, null otherwise.</returns>
    Task<TokenInfo?> GetAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates a token (marks it as logged out).
    /// </summary>
    /// <param name="token">The token to invalidate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if token was found and invalidated, false otherwise.</returns>
    Task<bool> InvalidateAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates all tokens for a specific user.
    /// </summary>
    /// <param name="username">The username whose tokens should be invalidated.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of tokens invalidated.</returns>
    Task<int> InvalidateAllForUserAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes expired tokens from the store.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of tokens removed.</returns>
    Task<int> CleanupExpiredAsync(CancellationToken cancellationToken = default);
}
