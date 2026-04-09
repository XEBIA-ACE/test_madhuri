namespace AuthenticationService.Core.DTOs;

/// <summary>
/// Response model for health check endpoint.
/// </summary>
public class HealthResponse
{
    public string Status { get; set; } = "Healthy";
    public string ServiceId { get; set; } = "AUTH-1";
    public string ServiceName { get; set; } = "Authentication Service";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, string> Dependencies { get; set; } = new();
}
