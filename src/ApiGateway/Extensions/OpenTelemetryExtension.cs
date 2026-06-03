using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

namespace ApiGateway.Extensions;

public static class OpenTelemetryExtensions
{   public static IServiceCollection AddOpenTelemetryTracing(
        this IServiceCollection services, IConfiguration config)
    {
        var serviceName = config["OTEL_SERVICE_NAME"] ?? 
                         config["Jaeger:ServiceName"] ?? 
                         "Unknown-Service";

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation(o => o.RecordException = true)
                    .AddHttpClientInstrumentation(o =>
                    {
                        o.RecordException = true;
                        o.FilterHttpRequestMessage = req => 
                            !(req.RequestUri?.ToString().Contains("/eureka/") ?? false);
                    });

                //Uncomment if this service uses EF Core
                tracing.AddEntityFrameworkCoreInstrumentation(o => o.SetDbStatementForText = true);

                tracing.AddOtlpExporter(otlp =>
{
    otlp.Endpoint = new Uri("http://jaeger:4317");
    otlp.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
    Log.Information("OpenTelemetry exporting to Jaeger via OTLP gRPC on port 4317");
});

            });

        Log.Information("✅ OpenTelemetry started → Service: {ServiceName}", serviceName);
        return services;
    }
}