using System.Collections.Concurrent;

namespace AuthenticationService.Services;

/// <summary>
/// Tracks authentication-related metrics.
/// </summary>
public interface IAuthenticationMetrics
{
    /// <summary>
    /// Records a successful login.
    /// </summary>
    void RecordLoginSuccess();

    /// <summary>
    /// Records a failed login attempt.
    /// </summary>
    void RecordLoginFailure();

    /// <summary>
    /// Records a token validation.
    /// </summary>
    /// <param name="isValid">Whether the token was valid.</param>
    void RecordTokenValidation(bool isValid);

    /// <summary>
    /// Records a logout.
    /// </summary>
    void RecordLogout();

    /// <summary>
    /// Gets current metrics snapshot.
    /// </summary>
    AuthenticationMetricsSnapshot GetSnapshot();
}

/// <summary>
/// Snapshot of authentication metrics.
/// </summary>
public sealed class AuthenticationMetricsSnapshot
{
    public long TotalLoginAttempts { get; init; }
    public long SuccessfulLogins { get; init; }
    public long FailedLogins { get; init; }
    public long TokenValidations { get; init; }
    public long ValidTokens { get; init; }
    public long InvalidTokens { get; init; }
    public long Logouts { get; init; }
    public DateTime SnapshotTime { get; init; }
}

/// <summary>
/// In-memory implementation of authentication metrics.
/// TODO: Integrate with proper metrics system (e.g., Prometheus, Application Insights) for production.
/// </summary>
public sealed class AuthenticationMetrics : IAuthenticationMetrics
{
    private long _successfulLogins;
    private long _failedLogins;
    private long _validTokenValidations;
    private long _invalidTokenValidations;
    private long _logouts;

    private readonly ILogger<AuthenticationMetrics> _logger;

    public AuthenticationMetrics(ILogger<AuthenticationMetrics> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public void RecordLoginSuccess()
    {
        Interlocked.Increment(ref _successfulLogins);
        _logger.LogDebug("Metric: Login success recorded");
    }

    /// <inheritdoc />
    public void RecordLoginFailure()
    {
        Interlocked.Increment(ref _failedLogins);
        _logger.LogDebug("Metric: Login failure recorded");
    }

    /// <inheritdoc />
    public void RecordTokenValidation(bool isValid)
    {
        if (isValid)
        {
            Interlocked.Increment(ref _validTokenValidations);
        }
        else
        {
            Interlocked.Increment(ref _invalidTokenValidations);
        }
        _logger.LogDebug("Metric: Token validation recorded (valid: {IsValid})", isValid);
    }

    /// <inheritdoc />
    public void RecordLogout()
    {
        Interlocked.Increment(ref _logouts);
        _logger.LogDebug("Metric: Logout recorded");
    }

    /// <inheritdoc />
    public AuthenticationMetricsSnapshot GetSnapshot()
    {
        return new AuthenticationMetricsSnapshot
        {
            TotalLoginAttempts = Interlocked.Read(ref _successfulLogins) + Interlocked.Read(ref _failedLogins),
            SuccessfulLogins = Interlocked.Read(ref _successfulLogins),
            FailedLogins = Interlocked.Read(ref _failedLogins),
            TokenValidations = Interlocked.Read(ref _validTokenValidations) + Interlocked.Read(ref _invalidTokenValidations),
            ValidTokens = Interlocked.Read(ref _validTokenValidations),
            InvalidTokens = Interlocked.Read(ref _invalidTokenValidations),
            Logouts = Interlocked.Read(ref _logouts),
            SnapshotTime = DateTime.UtcNow
        };
    }
}
