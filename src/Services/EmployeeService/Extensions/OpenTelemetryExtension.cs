using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Extensions.Hosting;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

namespace EmployeeService.Extensions;

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
                    // Register custom ActivitySource for RabbitMQ message publishing
                    // Links to code in RabbitMQPublisher.cs that creates publish spans
                    .AddSource("rabbitmq.publisher")
                    
                    // Track incoming HTTP requests (create order, get orders, etc.)
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;  // Capture exceptions in traces
                    })
                    
                    // Track outgoing HTTP calls to UserService (user validation)
                    .AddHttpClientInstrumentation(options =>
                    {
                        options.RecordException = true;  // Capture errors when calling UserService
                        
                        // Filter out Eureka service discovery calls from traces
                        // Eureka heartbeats/registration would clutter Jaeger UI
                        options.FilterHttpRequestMessage = (httpRequestMessage) =>
                        {
                            return !httpRequestMessage.RequestUri?.ToString().Contains("/eureka/") ?? true;
                        };
                    })
                    
                    // Track database queries via Entity Framework Core
                    // Shows SQL queries (INSERT order, SELECT orders) and execution time
                    .AddEntityFrameworkCoreInstrumentation(options =>
                    {
                        options.SetDbStatementForText = true;  // Include SQL query text in traces
                    })
                    
                    // Send traces to Jaeger backend via OTLP HTTP
                    .AddOtlpExporter(otlp =>
{
    otlp.Endpoint = new Uri("http://jaeger:4317");
    otlp.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
    Log.Information("OpenTelemetry exporting to Jaeger via OTLP gRPC on port 4317");
});

            });

        Log.Information("OpenTelemetry distributed tracing configured for EmployeeService");
        return services;
    }
}

