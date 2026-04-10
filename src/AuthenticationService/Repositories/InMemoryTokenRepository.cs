using System.Collections.Concurrent;
using AuthenticationService.Models.Entities;

namespace AuthenticationService.Repositories;

/// <summary>
/// In-memory implementation of the token repository for development and testing.
/// TODO: Replace with Redis or database-backed implementation for production.
/// </summary>
public class InMemoryTokenRepository : ITokenRepository
{
    private readonly ConcurrentDictionary<string, TokenRecord> _tokens = new();

    /// <inheritdoc />
    public Task AddAsync(TokenRecord tokenRecord)
    {
        tokenRecord.Id = Guid.NewGuid();
        _tokens[tokenRecord.TokenId] = tokenRecord;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<TokenRecord?> GetByTokenIdAsync(string tokenId)
    {
        _tokens.TryGetValue(tokenId, out var tokenRecord);
        return Task.FromResult(tokenRecord);
    }

    /// <inheritdoc />
    public Task<bool> InvalidateAsync(string tokenId)
    {
        if (_tokens.TryGetValue(tokenId, out var tokenRecord))
        {
            tokenRecord.IsInvalidated = true;
            tokenRecord.InvalidatedAt = DateTime.UtcNow;
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    /// <inheritdoc />
    public Task<bool> IsInvalidatedAsync(string tokenId)
    {
        if (_tokens.TryGetValue(tokenId, out var tokenRecord))
        {
            return Task.FromResult(tokenRecord.IsInvalidated);
        }
        // If token not found in store, consider it not invalidated
        // (it may be a valid token that was never stored, or an invalid token)
        return Task.FromResult(false);
    }

    /// <inheritdoc />
    public Task InvalidateAllForUserAsync(Guid userId)
    {
        var userTokens = _tokens.Values.Where(t => t.UserId == userId && !t.IsInvalidated);
        foreach (var token in userTokens)
        {
            token.IsInvalidated = true;
            token.InvalidatedAt = DateTime.UtcNow;
        }
        return Task.CompletedTask;
    }
}
