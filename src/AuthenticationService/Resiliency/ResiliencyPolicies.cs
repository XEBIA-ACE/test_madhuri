using Polly;
using Polly.Retry;
using Polly.Timeout;

namespace AuthenticationService.Resiliency;

/// <summary>
/// Provides resiliency policies for data store operations.
/// </summary>
public static class ResiliencyPolicies
{
    /// <summary>
    /// Default timeout for data store operations.
    /// </summary>
    public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Creates a retry policy for transient failures.
    /// </summary>
    public static ResiliencePipeline CreateRetryPipeline(ILogger logger)
    {
        return new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromMilliseconds(100),
                BackoffType = DelayBackoffType.Exponential,
                OnRetry = args =>
                {
                    logger.LogWarning(
                        "Retry attempt {AttemptNumber} after {Delay}ms due to: {Exception}",
                        args.AttemptNumber,
                        args.RetryDelay.TotalMilliseconds,
                        args.Outcome.Exception?.Message ?? "Unknown");
                    return ValueTask.CompletedTask;
                }
            })
            .AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = DefaultTimeout,
                OnTimeout = args =>
                {
                    logger.LogWarning(
                        "Operation timed out after {Timeout}ms",
                        args.Timeout.TotalMilliseconds);
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
    }

    /// <summary>
    /// Creates a timeout-only policy for operations that shouldn't be retried.
    /// </summary>
    public static ResiliencePipeline CreateTimeoutPipeline(ILogger logger, TimeSpan? timeout = null)
    {
        return new ResiliencePipelineBuilder()
            .AddTimeout(new TimeoutStrategyOptions
            {
                Timeout = timeout ?? DefaultTimeout,
                OnTimeout = args =>
                {
                    logger.LogWarning(
                        "Operation timed out after {Timeout}ms",
                        args.Timeout.TotalMilliseconds);
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
    }
}
