namespace Shared.Common.Configuration;

/// <summary>
/// Configuration settings for Jaeger distributed tracing
/// Maps to "Jaeger" section in appsettings.json
/// </summary>
public class JaegerSettings
{
    public string ServiceName { get; set; } = string.Empty;
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 6831;
    public double SamplingRate { get; set; } = 1.0;  // Sample all traces
}
