using AuthenticationService.Models.Entities;
using AuthenticationService.Resiliency;
using Polly;

namespace AuthenticationService.Stores;

/// <summary>
/// Decorator that adds resiliency to token store operations.
/// </summary>
public sealed class ResilientTokenStore : ITokenStore
{
    private readonly ITokenStore _innerStore;
    private readonly ResiliencePipeline _pipeline;
    private readonly ILogger<ResilientTokenStore> _logger;

    public ResilientTokenStore(
        ITokenStore innerStore,
        ILogger<ResilientTokenStore> logger)
    {
        _innerStore = innerStore;
        _logger = logger;
        _pipeline = ResiliencyPolicies.CreateRetryPipeline(logger);
    }

    /// <inheritdoc />
    public async Task StoreAsync(TokenInfo tokenInfo, CancellationToken cancellationToken = default)
    {
        await _pipeline.ExecuteAsync(
            async ct =>
            {
                await _innerStore.StoreAsync(tokenInfo, ct);
                return true; // Polly requires a return value
            },
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<TokenInfo?> GetAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _pipeline.ExecuteAsync(
            async ct => await _innerStore.GetAsync(token, ct),
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> InvalidateAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _pipeline.ExecuteAsync(
            async ct => await _innerStore.InvalidateAsync(token, ct),
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> InvalidateAllForUserAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _pipeline.ExecuteAsync(
            async ct => await _innerStore.InvalidateAllForUserAsync(username, ct),
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> CleanupExpiredAsync(CancellationToken cancellationToken = default)
    {
        return await _pipeline.ExecuteAsync(
            async ct => await _innerStore.CleanupExpiredAsync(ct),
            cancellationToken);
    }
}
