using Microsoft.AspNetCore.Mvc;

namespace AuthenticationService.Controllers;

/// <summary>
/// Controller for health and readiness checks.
/// </summary>
[ApiController]
[Route("api")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Health check endpoint.
    /// </summary>
    /// <returns>Health status.</returns>
    [HttpGet("health")]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(new HealthResponse
        {
            Status = "healthy",
            ServiceId = "AUTH-1",
            ServiceName = "Authentication Service",
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Readiness check endpoint.
    /// </summary>
    /// <returns>Readiness status.</returns>
    [HttpGet("ready")]
    [ProducesResponseType(typeof(ReadinessResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ReadinessResponse), StatusCodes.Status503ServiceUnavailable)]
    public IActionResult Ready()
    {
        // TODO: Add actual readiness checks (database connectivity, etc.)
        var isReady = true;

        var response = new ReadinessResponse
        {
            Ready = isReady,
            ServiceId = "AUTH-1",
            ServiceName = "Authentication Service",
            Timestamp = DateTime.UtcNow,
            Checks = new Dictionary<string, bool>
            {
                { "database", true }, // TODO: Implement actual check
                { "dependencies", true } // TODO: Implement actual check
            }
        };

        if (!isReady)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, response);
        }

        return Ok(response);
    }
}

/// <summary>
/// Health check response.
/// </summary>
public class HealthResponse
{
    public string Status { get; set; } = string.Empty;
    public string ServiceId { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Readiness check response.
/// </summary>
public class ReadinessResponse
{
    public bool Ready { get; set; }
    public string ServiceId { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Dictionary<string, bool> Checks { get; set; } = new();
}
