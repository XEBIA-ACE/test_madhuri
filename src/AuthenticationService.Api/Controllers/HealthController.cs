using AuthenticationService.Core.Configuration;
using AuthenticationService.Core.DTOs;
using AuthenticationService.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AuthenticationService.Api.Controllers;

/// <summary>
/// Controller for health check endpoints.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class HealthController : ControllerBase
{
    private readonly AuthDbContext _dbContext;
    private readonly ServiceSettings _serviceSettings;
    private readonly ILogger<HealthController> _logger;

    public HealthController(
        AuthDbContext dbContext,
        IOptions<ServiceSettings> serviceSettings,
        ILogger<HealthController> logger)
    {
        _dbContext = dbContext;
        _serviceSettings = serviceSettings.Value;
        _logger = logger;
    }

    /// <summary>
    /// Get service health status.
    /// </summary>
    /// <returns>Health status</returns>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetHealth()
    {
        var response = new HealthResponse
        {
            ServiceId = _serviceSettings.ServiceId,
            ServiceName = _serviceSettings.ServiceName,
            Timestamp = DateTime.UtcNow,
            Dependencies = new Dictionary<string, string>()
        };

        try
        {
            // Check database connectivity
            var canConnect = await _dbContext.Database.CanConnectAsync();
            response.Dependencies["Database"] = canConnect ? "Healthy" : "Unhealthy";

            if (!canConnect)
            {
                response.Status = "Unhealthy";
                _logger.LogWarning("Health check failed - database connection unavailable");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, response);
            }

            response.Status = "Healthy";
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed with exception");
            response.Status = "Unhealthy";
            response.Dependencies["Database"] = "Error";
            return StatusCode(StatusCodes.Status503ServiceUnavailable, response);
        }
    }

    /// <summary>
    /// Get service readiness status.
    /// </summary>
    /// <returns>Readiness status</returns>
    [HttpGet("ready")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetReadiness()
    {
        try
        {
            var canConnect = await _dbContext.Database.CanConnectAsync();
            if (canConnect)
            {
                return Ok(new { Status = "Ready" });
            }
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { Status = "Not Ready" });
        }
        catch
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { Status = "Not Ready" });
        }
    }

    /// <summary>
    /// Get service liveness status.
    /// </summary>
    /// <returns>Liveness status</returns>
    [HttpGet("live")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetLiveness()
    {
        return Ok(new { Status = "Alive" });
    }
}
