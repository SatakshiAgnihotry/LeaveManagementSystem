using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Extensions.Hosting;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

namespace NotificationService.Extensions;

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
                    // Register custom ActivitySource for RabbitMQ message consumption
                    // Links to code in OrderCreatedConsumer.cs that creates consumer spans
                    .AddSource("rabbitmq.consumer")
                    
                    // Track HTTP requests (mainly health endpoint)
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;  // Capture exceptions in traces
                    })
                    
                    // Send traces to Jaeger backend via OTLP HTTP
                    .AddOtlpExporter(otlp =>
{
    otlp.Endpoint = new Uri("http://jaeger:4317");
    otlp.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
    Log.Information("OpenTelemetry exporting to Jaeger via OTLP gRPC on port 4317");
});

            });

        Log.Information("OpenTelemetry distributed tracing configured for NotificationService");
        return services;
    }
}

