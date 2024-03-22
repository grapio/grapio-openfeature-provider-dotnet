using Microsoft.Extensions.DependencyInjection;

namespace Grapio.Provider;

/// <summary>
/// Provides extensions for the Grapio Provider.
/// </summary>
public static class GrapioExtensions
{
    /// <summary>
    /// Allows the Grapio Provider to be configured.
    /// </summary>
    /// <param name="services">The service collection to register the <see cref="GrapioProvider"/> singleton.</param>
    /// <param name="config">Action that can be used to configure the <see cref="GrapioProvider"/>.</param>
    public static void AddGrapio(this IServiceCollection services, Action<IGrapioConfiguration> config)
    {
        var configuration = new GrapioConfiguration();
        config(configuration);
        configuration.ValidateAndThrow();

        services.AddSingleton<IGrapioConfiguration>(configuration);
        services.AddSingleton<GrapioProvider>();
    }
    
    /// <summary>
    /// Allows the Grapio Provider to be configured.
    /// </summary>
    /// <param name="services">The service collection to register the <see cref="GrapioProvider"/> singleton.</param>
    /// <param name="config"><see cref="GrapioConfiguration"/> for the <see cref="GrapioProvider"/>.</param>
    public static void AddGrapio(this IServiceCollection services, IGrapioConfiguration config)
    {
        config.ValidateAndThrow();

        services.AddSingleton(config);
        services.AddSingleton<GrapioProvider>();
    }
}
