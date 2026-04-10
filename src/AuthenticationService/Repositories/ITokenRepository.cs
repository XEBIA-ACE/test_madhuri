using AuthenticationService.Models.Entities;

namespace AuthenticationService.Repositories;

/// <summary>
/// Repository interface for token/session data access.
/// </summary>
public interface ITokenRepository
{
    /// <summary>
    /// Stores a new token record.
    /// </summary>
    /// <param name="tokenRecord">The token record to store.</param>
    Task AddAsync(TokenRecord tokenRecord);

    /// <summary>
    /// Retrieves a token record by its token ID (jti claim).
    /// </summary>
    /// <param name="tokenId">The token ID to search for.</param>
    /// <returns>The token record if found, null otherwise.</returns>
    Task<TokenRecord?> GetByTokenIdAsync(string tokenId);

    /// <summary>
    /// Invalidates a token by its token ID.
    /// </summary>
    /// <param name="tokenId">The token ID to invalidate.</param>
    /// <returns>True if the token was found and invalidated, false otherwise.</returns>
    Task<bool> InvalidateAsync(string tokenId);

    /// <summary>
    /// Checks if a token has been invalidated.
    /// </summary>
    /// <param name="tokenId">The token ID to check.</param>
    /// <returns>True if the token is invalidated, false otherwise.</returns>
    Task<bool> IsInvalidatedAsync(string tokenId);

    /// <summary>
    /// Invalidates all tokens for a specific user.
    /// </summary>
    /// <param name="userId">The user ID whose tokens should be invalidated.</param>
    Task InvalidateAllForUserAsync(Guid userId);
}
