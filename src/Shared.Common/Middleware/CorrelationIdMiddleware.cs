using System.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace Shared.Common.Middleware;

/// <summary>
/// Middleware that adds a unique correlation ID to each request for distributed tracing
/// Allows tracking of a single logical request flow across multiple microservices
/// The correlation ID is added to response headers and can be used in logs
/// </summary>
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeader = "X-Correlation-ID";

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Check if correlation ID already exists (from upstream service)
        if (!context.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationId))
        {
            // Generate new correlation ID if not present
            correlationId = Activity.Current?.Id ?? context.TraceIdentifier;
        }

        // Add correlation ID to response headers for client visibility
        context.Response.OnStarting(() =>
        {
            if (!context.Response.Headers.ContainsKey(CorrelationIdHeader))
            {
                context.Response.Headers.Add(CorrelationIdHeader, correlationId);
            }
            return Task.CompletedTask;
        });

        // Add correlation ID to items for access in service code
        context.Items[CorrelationIdHeader] = correlationId;

        await _next(context);
    }
}
