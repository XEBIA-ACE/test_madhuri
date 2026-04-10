namespace AuthenticationService.Models.Responses;

/// <summary>
/// Standard error response model.
/// </summary>
public sealed class ErrorResponse
{
    /// <summary>
    /// Error code for programmatic handling.
    /// </summary>
    public required string ErrorCode { get; init; }

    /// <summary>
    /// Human-readable error message.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Optional additional details about the error.
    /// </summary>
    public IDictionary<string, string[]>? Details { get; init; }

    /// <summary>
    /// Timestamp when the error occurred.
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
