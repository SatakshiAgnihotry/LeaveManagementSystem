using IdentityService.Data;
using IdentityService.Middleware;
using IdentityService.Services;
using IdentityService.Extensions;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Shared.Common.Extensions;
using Shared.Common.Middleware;

// ========== LOGGING CONFIGURATION ==========
// OLD CODE:
// Log.Logger =new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddEnvironmentVariables()
        .Build())
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "IdentityService")
    .WriteTo.Console()
    .WriteTo.File("logs/identityservice-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

// ========== SERVICE REGISTRATION ==========
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ========== JWT AUTHENTICATION ==========
builder.Services.AddJwtAuthentication(builder.Configuration);

// ========== DATABASE CONFIGURATION ==========
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IAuthService, AuthService>();

// ========== SERVICE DISCOVERY ==========
// OLD CODE:
// builder.Services.AddDiscoveryClient(builder.Configuration);
builder.Services.AddEurekaServiceDiscovery(builder.Configuration);

// ========== OPENTELEMETRY & JAEGER TRACING ==========
builder.Services.AddOpenTelemetryTracing(builder.Configuration);

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

builder.Services.AddHealthChecks();

var app = builder.Build();

// ========== DATABASE INITIALIZATION ==========
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// ========== MIDDLEWARE PIPELINE ==========
app.UseCors();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ServiceInstanceMiddleware>();
app.UseMiddleware<HealthCheckMiddleware>("IdentityService");
app.UseMiddleware<ExceptionalHandlingMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

try
{
    Log.Information("Starting Identity Service");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Identity Service terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}