using AuthenticationService.Models.Entities;
using AuthenticationService.Resiliency;
using Polly;

namespace AuthenticationService.Stores;

/// <summary>
/// Decorator that adds resiliency to user store operations.
/// </summary>
public sealed class ResilientUserStore : IUserStore
{
    private readonly IUserStore _innerStore;
    private readonly ResiliencePipeline _pipeline;
    private readonly ILogger<ResilientUserStore> _logger;

    public ResilientUserStore(
        IUserStore innerStore,
        ILogger<ResilientUserStore> logger)
    {
        _innerStore = innerStore;
        _logger = logger;
        _pipeline = ResiliencyPolicies.CreateRetryPipeline(logger);
    }

    /// <inheritdoc />
    public async Task<UserCredentials?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _pipeline.ExecuteAsync(
            async ct => await _innerStore.GetByUsernameAsync(username, ct),
            cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _pipeline.ExecuteAsync(
            async ct => await _innerStore.ExistsAsync(username, ct),
            cancellationToken);
    }
}
