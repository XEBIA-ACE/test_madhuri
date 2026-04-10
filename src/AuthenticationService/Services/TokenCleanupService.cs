using AuthenticationService.Stores;

namespace AuthenticationService.Services;

/// <summary>
/// Background service that periodically cleans up expired tokens.
/// </summary>
public sealed class TokenCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TokenCleanupService> _logger;
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(5);

    public TokenCleanupService(
        IServiceProvider serviceProvider,
        ILogger<TokenCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Token cleanup service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_cleanupInterval, stoppingToken);
                await CleanupExpiredTokensAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Expected during shutdown
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token cleanup");
            }
        }

        _logger.LogInformation("Token cleanup service stopped");
    }

    private async Task CleanupExpiredTokensAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var tokenStore = scope.ServiceProvider.GetRequiredService<ITokenStore>();

        var removedCount = await tokenStore.CleanupExpiredAsync(cancellationToken);

        if (removedCount > 0)
        {
            _logger.LogInformation("Cleaned up {Count} expired tokens", removedCount);
        }
    }
}
