using Microsoft.AspNetCore.Http;

namespace Shared.Common.Middleware;

/// <summary>
/// Middleware that intercepts /health endpoint requests
/// Returns a standardized health status JSON response
/// Used by load balancers and service discovery (Eureka) to check service health
/// </summary>
public class HealthCheckMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _serviceName;
    private readonly string _discoveryMode;

    public HealthCheckMiddleware(RequestDelegate next, string serviceName, string discoveryMode = "Static")
    {
        _next = next;
        _serviceName = serviceName;
        _discoveryMode = discoveryMode;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Check if this is a health check request
        if (context.Request.Path.Equals("/health", StringComparison.OrdinalIgnoreCase))
        {
            var healthStatus = new
            {
                status = "UP",
                service = _serviceName,
                discoveryMode = _discoveryMode,
                timestamp = DateTime.UtcNow,
                uptime = GetUptime()
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status200OK;
            await context.Response.WriteAsJsonAsync(healthStatus);
            return;
        }

        await _next(context);
    }

    private static string GetUptime()
    {
        // Return process uptime for monitoring
        var uptime = DateTime.Now - System.Diagnostics.Process.GetCurrentProcess().StartTime;
        return $"{uptime.Days}d {uptime.Hours}h {uptime.Minutes}m";
    }
}
