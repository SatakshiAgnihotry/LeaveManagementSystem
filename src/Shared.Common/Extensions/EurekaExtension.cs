using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Steeltoe.Discovery.Client;
using Shared.Common.Configuration;

namespace Shared.Common.Extensions;

/// <summary>
/// Extension method for registering Eureka service discovery
/// Enables microservices to register themselves and discover other services dynamically
/// Reads settings from appsettings.json "Eureka" section
/// </summary>
public static class EurekaExtension
{
    public static IServiceCollection AddEurekaServiceDiscovery(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind Eureka configuration from appsettings.json
        services.Configure<EurekaSettings>(configuration.GetSection("Eureka"));

        // Register Steeltoe Discovery Client
        // This automatically handles service registration and discovery
        services.AddDiscoveryClient(configuration);

        return services;
    }

    /// <summary>
    /// Add Steeltoe's HttpClientFactory integration for Eureka-based service discovery
    /// This allows HttpClient to resolve service names to actual URLs via Eureka
    /// </summary>
    public static IServiceCollection AddServiceDiscoveryHttpClient(
        this IServiceCollection services)
    {
        // Use Steeltoe's DiscoveryHttpClientHandler for service name resolution
        services.AddHttpClient();
        return services;
    }
}
