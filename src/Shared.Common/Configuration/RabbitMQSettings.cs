namespace Shared.Common.Configuration;

/// <summary>
/// Configuration settings for RabbitMQ message broker
/// Maps to "RabbitMQ" section in appsettings.json
/// </summary>
public class RabbitMQSettings
{
    public string HostName { get; set; } = "rabbitmq";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public string ExchangeName { get; set; } = "lms.exchange";
    public string RoutingKey { get; set; } = "lms.routing.key";
    public int MessageTtlMilliseconds { get; set; } = 86400000;  // 24 hours
}
