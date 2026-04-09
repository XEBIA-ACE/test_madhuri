using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;
using AuthenticationService.Core.DTOs;

namespace AuthenticationService.Api.Middleware;

/// <summary>
/// Simple rate limiting middleware for brute-force protection.
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly ConcurrentDictionary<string, RateLimitInfo> _clients;
    private readonly int _maxRequests;
    private readonly TimeSpan _timeWindow;

    public RateLimitingMiddleware(
        RequestDelegate next,
        ILogger<RateLimitingMiddleware> logger,
        int maxRequests = 100,
        int timeWindowSeconds = 60)
    {
        _next = next;
        _logger = logger;
        _clients = new ConcurrentDictionary<string, RateLimitInfo>();
        _maxRequests = maxRequests;
        _timeWindow = TimeSpan.FromSeconds(timeWindowSeconds);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = GetClientIdentifier(context);
        var now = DateTime.UtcNow;

        var rateLimitInfo = _clients.AddOrUpdate(
            clientId,
            _ => new RateLimitInfo { RequestCount = 1, WindowStart = now },
            (_, existing) =>
            {
                if (now - existing.WindowStart > _timeWindow)
                {
                    return new RateLimitInfo { RequestCount = 1, WindowStart = now };
                }
                existing.RequestCount++;
                return existing;
            });

        if (rateLimitInfo.RequestCount > _maxRequests)
        {
            _logger.LogWarning("Rate limit exceeded for client: {ClientId}", clientId);
            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.ContentType = "application/json";

            var errorResponse = new ErrorResponse
            {
                Error = "Rate limit exceeded. Please try again later.",
                StatusCode = (int)HttpStatusCode.TooManyRequests
            };

            var json = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
            return;
        }

        await _next(context);
    }

    private static string GetClientIdentifier(HttpContext context)
    {
        // Use X-Forwarded-For header if behind a proxy, otherwise use remote IP
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private class RateLimitInfo
    {
        public int RequestCount { get; set; }
        public DateTime WindowStart { get; set; }
    }
}

/// <summary>
/// Extension methods for rate limiting middleware.
/// </summary>
public static class RateLimitingMiddlewareExtensions
{
    public static IApplicationBuilder UseRateLimiting(
        this IApplicationBuilder builder,
        int maxRequests = 100,
        int timeWindowSeconds = 60)
    {
        return builder.UseMiddleware<RateLimitingMiddleware>(maxRequests, timeWindowSeconds);
    }
}
