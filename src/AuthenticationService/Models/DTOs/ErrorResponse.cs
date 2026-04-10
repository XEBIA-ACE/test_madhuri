namespace AuthenticationService.Models.DTOs;

/// <summary>
/// Standard error response model.
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// Error code for programmatic handling.
    /// </summary>
    public required string ErrorCode { get; set; }

    /// <summary>
    /// Human-readable error message.
    /// </summary>
    public required string Message { get; set; }

    /// <summary>
    /// Additional details about the error, if available.
    /// </summary>
    public Dictionary<string, string[]>? Details { get; set; }

    /// <summary>
    /// Timestamp when the error occurred.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
