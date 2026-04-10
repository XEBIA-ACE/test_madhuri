using System.Collections.Concurrent;
using AuthenticationService.Models.Entities;

namespace AuthenticationService.Stores;

/// <summary>
/// In-memory implementation of token store for development/testing.
/// TODO: Replace with Redis or distributed cache for production.
/// </summary>
public sealed class InMemoryTokenStore : ITokenStore
{
    private readonly ConcurrentDictionary<string, TokenInfo> _tokens = new();
    private readonly ILogger<InMemoryTokenStore> _logger;

    public InMemoryTokenStore(ILogger<InMemoryTokenStore> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public Task StoreAsync(TokenInfo tokenInfo, CancellationToken cancellationToken = default)
    {
        _tokens[tokenInfo.Token] = tokenInfo;
        _logger.LogDebug("Stored token for user: {Username}, expires: {ExpiresAt}", 
            tokenInfo.Username, tokenInfo.ExpiresAt);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<TokenInfo?> GetAsync(string token, CancellationToken cancellationToken = default)
    {
        _tokens.TryGetValue(token, out var tokenInfo);
        return Task.FromResult(tokenInfo);
    }

    /// <inheritdoc />
    public Task<bool> InvalidateAsync(string token, CancellationToken cancellationToken = default)
    {
        if (_tokens.TryGetValue(token, out var tokenInfo))
        {
            tokenInfo.IsInvalidated = true;
            _logger.LogDebug("Invalidated token for user: {Username}", tokenInfo.Username);
            return Task.FromResult(true);
        }

        _logger.LogDebug("Token not found for invalidation");
        return Task.FromResult(false);
    }

    /// <inheritdoc />
    public Task<int> InvalidateAllForUserAsync(string username, CancellationToken cancellationToken = default)
    {
        var count = 0;
        foreach (var kvp in _tokens)
        {
            if (string.Equals(kvp.Value.Username, username, StringComparison.OrdinalIgnoreCase) && !kvp.Value.IsInvalidated)
            {
                kvp.Value.IsInvalidated = true;
                count++;
            }
        }

        _logger.LogDebug("Invalidated {Count} tokens for user: {Username}", count, username);
        return Task.FromResult(count);
    }

    /// <inheritdoc />
    public Task<int> CleanupExpiredAsync(CancellationToken cancellationToken = default)
    {
        var count = 0;
        var now = DateTime.UtcNow;

        foreach (var kvp in _tokens)
        {
            if (kvp.Value.ExpiresAt < now)
            {
                if (_tokens.TryRemove(kvp.Key, out _))
                {
                    count++;
                }
            }
        }

        if (count > 0)
        {
            _logger.LogInformation("Cleaned up {Count} expired tokens", count);
        }

        return Task.FromResult(count);
    }
}
