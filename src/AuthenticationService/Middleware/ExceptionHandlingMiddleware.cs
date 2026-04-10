using System.Net;
using System.Text.Json;
using AuthenticationService.Models.Responses;

namespace AuthenticationService.Middleware;

/// <summary>
/// Global exception handling middleware for consistent error responses.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);

        var (statusCode, errorCode, message) = exception switch
        {
            ArgumentException => (HttpStatusCode.BadRequest, "INVALID_ARGUMENT", exception.Message),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "UNAUTHORIZED", "Access denied"),
            OperationCanceledException => (HttpStatusCode.RequestTimeout, "REQUEST_TIMEOUT", "Request was cancelled"),
            TimeoutException => (HttpStatusCode.GatewayTimeout, "TIMEOUT", "Operation timed out"),
            _ => (HttpStatusCode.InternalServerError, "INTERNAL_ERROR", "An unexpected error occurred")
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var errorResponse = new ErrorResponse
        {
            ErrorCode = errorCode,
            Message = message,
            Timestamp = DateTime.UtcNow
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse, options));
    }
}

/// <summary>
/// Extension methods for exception handling middleware.
/// </summary>
public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
