using AuthenticationService.Models.Responses;
using System.Text.Json;

namespace AuthenticationService.Middleware;

/// <summary>
/// Middleware to enforce HTTPS connections.
/// </summary>
public sealed class HttpsEnforcementMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<HttpsEnforcementMiddleware> _logger;
    private readonly bool _enforceHttps;

    public HttpsEnforcementMiddleware(
        RequestDelegate next,
        ILogger<HttpsEnforcementMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _enforceHttps = configuration.GetValue("Security:EnforceHttps", false);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip enforcement in development or if disabled
        if (!_enforceHttps)
        {
            await _next(context);
            return;
        }

        // Allow health checks over HTTP
        if (context.Request.Path.StartsWithSegments("/health"))
        {
            await _next(context);
            return;
        }

        if (!context.Request.IsHttps)
        {
            _logger.LogWarning(
                "Rejected non-HTTPS request to {Path}",
                context.Request.Path);

            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";

            var errorResponse = new ErrorResponse
            {
                ErrorCode = "HTTPS_REQUIRED",
                Message = "HTTPS is required for all API endpoints",
                Timestamp = DateTime.UtcNow
            };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse, options));
            return;
        }

        await _next(context);
    }
}

/// <summary>
/// Extension methods for HTTPS enforcement middleware.
/// </summary>
public static class HttpsEnforcementMiddlewareExtensions
{
    public static IApplicationBuilder UseHttpsEnforcement(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<HttpsEnforcementMiddleware>();
    }
}
