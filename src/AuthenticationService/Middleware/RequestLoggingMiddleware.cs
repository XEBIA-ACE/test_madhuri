using System.Diagnostics;

namespace AuthenticationService.Middleware;

/// <summary>
/// Middleware for logging HTTP requests and responses with timing metrics.
/// </summary>
public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var requestId = Guid.NewGuid().ToString("N")[..8];
        var stopwatch = Stopwatch.StartNew();

        // Add request ID to response headers for tracing
        context.Response.Headers["X-Request-Id"] = requestId;

        _logger.LogInformation(
            "[{RequestId}] {Method} {Path} - Request started",
            requestId,
            context.Request.Method,
            context.Request.Path);

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            var logLevel = context.Response.StatusCode >= 500 ? LogLevel.Error :
                           context.Response.StatusCode >= 400 ? LogLevel.Warning :
                           LogLevel.Information;

            _logger.Log(
                logLevel,
                "[{RequestId}] {Method} {Path} - {StatusCode} in {ElapsedMs}ms",
                requestId,
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds);

            // Log warning if response time exceeds 500ms (per spec requirement)
            if (stopwatch.ElapsedMilliseconds > 500)
            {
                _logger.LogWarning(
                    "[{RequestId}] Response time {ElapsedMs}ms exceeded 500ms threshold",
                    requestId,
                    stopwatch.ElapsedMilliseconds);
            }
        }
    }
}

/// <summary>
/// Extension methods for request logging middleware.
/// </summary>
public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLoggingMiddleware>();
    }
}
