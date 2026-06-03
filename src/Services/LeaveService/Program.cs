using System.Text;
using LeaveService.Data;
using LeaveService.Extensions;
using LeaveService.Middleware;
using LeaveService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Http;
using Microsoft.IdentityModel.Tokens;
using Polly;
using Serilog;
using Steeltoe.Discovery.Client;


Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddEnvironmentVariables()
        .Build())
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "LeaveService")
    .WriteTo.Console()
    .WriteTo.File("logs/leaveservice-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDiscoveryClient(builder.Configuration);
builder.Services.AddOpenTelemetryTracing(builder.Configuration);

// EF Core with PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT Validation
var jwtSecret = builder.Configuration["Jwt:SecretKey"]!;
var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

builder.Services.AddAuthorization();

// LeaveRepository — scoped because it uses DbContext
builder.Services.AddScoped<ILeaveRepository, LeaveRepository>();

// RabbitMQ publisher — singleton because one connection serves the whole app
builder.Services.AddSingleton<IRabbitMQPublisher, RabbitMQPublisher>();

// Register IHttpContextAccessor
builder.Services.AddHttpContextAccessor();

// EmployeeServiceClient with Polly — retry 3x with exponential backoff + circuit breaker
builder.Services.AddHttpClient<IEmployeeServiceClient, EmployeeServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:EmployeeServiceUrl"]!);
    client.DefaultRequestHeaders.Add("X-Service-Api-Key", builder.Configuration["ServiceApiKey"]!);
    client.Timeout = TimeSpan.FromSeconds(30);  // Overall request timeout
})
.AddResiliencePolicies(builder.Configuration);


builder.Services.AddHealthChecks();

var app = builder.Build();

// Auto-run migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();