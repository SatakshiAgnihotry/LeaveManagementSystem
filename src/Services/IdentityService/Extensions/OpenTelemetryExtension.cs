using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Extensions.Hosting;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
namespace IdentityService.Extensions;

public static class OpenTelemetryExtension
{
        public static IServiceCollection AddOpenTelemetryTracing(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var serviceName = configuration["OTEL_SERVICE_NAME"] ?? 
                         configuration["Jaeger:ServiceName"] ?? 
                         "Unknown-Service";

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName))
            .WithTracing(tracing =>
            {
                tracing
                    // Track incoming HTTP requests (login, get users, etc.)
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;  // Capture exceptions in traces
                    })
                    
                    // Track outgoing HTTP calls (if UserService calls other services)
                    .AddHttpClientInstrumentation(options =>
                    {
                        options.RecordException = true;
                        
                        // Filter out Eureka service discovery calls from traces
                        // Eureka heartbeats/registration would clutter Jaeger UI
                        options.FilterHttpRequestMessage = (httpRequestMessage) =>
                        {
                            return !httpRequestMessage.RequestUri?.ToString().Contains("/eureka/") ?? true;
                        };
                    })
                    
                    // Track database queries via Entity Framework Core
                    // Shows SQL queries and their execution time in Jaeger UI
                    .AddEntityFrameworkCoreInstrumentation(options =>
                    {
                        options.SetDbStatementForText = true;  // Include SQL query text in traces
                    })
                    
                    // Send traces to Jaeger backend
                    .AddOtlpExporter(otlp =>
{
    otlp.Endpoint = new Uri("http://jaeger:4317");
    otlp.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
    Log.Information("OpenTelemetry exporting to Jaeger via OTLP gRPC on port 4317");
});

            });

        Log.Information("OpenTelemetry distributed tracing configured for IdentityService");
        return services;
    }
}

