using Microsoft.AspNetCore.Http;

namespace Shared.Common.Middleware;

/// <summary>
/// Middleware that adds service instance information to response headers
/// Useful for debugging and monitoring which instance handled the request
/// Helps track load balancing across multiple instances of the same service
/// </summary>
public class ServiceInstanceMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _instanceId;
    private const string InstanceIdHeader = "X-Instance-ID";

    public ServiceInstanceMiddleware(RequestDelegate next)
    {
        _next = next;
        // Generate unique instance ID (hostname + process ID)
        _instanceId = $"{Environment.MachineName}-{Environment.ProcessId}";
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Add instance ID to response headers
        context.Response.OnStarting(() =>
        {
            if (!context.Response.Headers.ContainsKey(InstanceIdHeader))
            {
                context.Response.Headers.Add(InstanceIdHeader, _instanceId);
            }
            return Task.CompletedTask;
        });

        await _next(context);
    }
}
