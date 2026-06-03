namespace Shared.Common.Configuration;

/// <summary>
/// Configuration settings for Eureka service discovery
/// Maps to "Eureka" section in appsettings.json
/// </summary>
public class EurekaSettings
{
    public ClientSettings Client { get; set; } = new();
    public InstanceSettings Instance { get; set; } = new();
}

public class ClientSettings
{
    public string ServiceUrl { get; set; } = "http://localhost:8761/eureka/";
    public bool ShouldRegisterWithEureka { get; set; } = true;
    public bool ShouldFetchRegistry { get; set; } = true;
}

public class InstanceSettings
{
    public string AppName { get; set; } = string.Empty;
    public int Port { get; set; } = 8080;
    public bool PreferIpAddress { get; set; } = false;
    public string StatusPageUrlPath { get; set; } = "/swagger";
    public string HealthCheckUrlPath { get; set; } = "/health";
}
