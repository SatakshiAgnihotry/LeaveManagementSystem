using ApiGateway.Extensions;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Serilog;
using Steeltoe.Discovery.Client;    
using Shared.Common.Middleware;
using Shared.Common.Extensions;
using Ocelot.Provider.Eureka;
// ========== OCELOT CONFIGURATION SELECTION ==========
// Dynamically choose between static routing (hardcoded URLs) and Eureka-based service discovery
// UseEureka flag is read from appsettings.json
var builder = WebApplication.CreateBuilder(args);

var useEureka = builder.Configuration.GetValue<bool>("UseEureka", false);
var ocelotConfigFile = useEureka ? "ocelot.eureka.json" : "ocelot.static.json";

// Load the selected Ocelot configuration file
// OLD CODE:
// .AddJsonFile("ocelot.eureka.json", optional: false, reloadOnChange: true)
builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile(ocelotConfigFile, optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

// ========== LOGGING CONFIGURATION ==========
// Configure Serilog for structured logging with multiple sinks
// OLD CODE:
// Log.Logger = new LoggerConfiguration()
//     .WriteTo.Console()
//     .CreateBootstrapLogger();
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "ApiGateway")
    .WriteTo.Console()
    .WriteTo.File("logs/apigateway-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

// OLD CODE:
// builder.Host.UseSerilog((ctx, lc) => lc
//     .ReadFrom.Configuration(ctx.Configuration)
//     .WriteTo.Console());
builder.Host.UseSerilog();

// ========== JWT AUTHENTICATION ==========
builder.Services.AddJwtAuthentication(builder.Configuration);

// ========== SERVICE DISCOVERY ==========
// OLD CODE:
// builder.Services.AddDiscoveryClient(builder.Configuration);
if (useEureka)
{
    builder.Services.AddEurekaServiceDiscovery(builder.Configuration);
    Log.Information("API Gateway configured with Eureka service discovery");
}
else
{
    Log.Information("API Gateway configured with static routing");
}

// ========== OCELOT API GATEWAY SETUP ==========
// OLD CODE:
// builder.Services.AddOcelot();
var ocelotBuilder = builder.Services.AddOcelot();

if (useEureka)
{
    // OLD CODE:
     ocelotBuilder.AddEureka();
    // Eureka provider is configured via ocelot.eureka.json ServiceDiscoveryProvider settings.
    Log.Information("Ocelot Eureka provider will be used via configuration file");
}

// ========== CORS CONFIGURATION ==========
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ========== OPENTELEMETRY & JAEGER TRACING ==========
builder.Services.AddOpenTelemetryTracing(builder.Configuration);

var app = builder.Build();

// ========== MIDDLEWARE PIPELINE ==========
app.UseCors();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ServiceInstanceMiddleware>();
app.UseMiddleware<HealthCheckMiddleware>("ApiGateway", useEureka ? "Eureka" : "Static");

app.UseAuthentication();
app.UseAuthorization();

try
{
    Log.Information("Starting API Gateway with {Mode} routing", useEureka ? "Eureka" : "Static");
    // OLD CODE:
    // await app.UseOcelot();
    await app.UseOcelot();
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "API Gateway terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
