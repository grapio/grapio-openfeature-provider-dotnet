using Microsoft.Extensions.Logging;

namespace Grapio.Provider;

public interface IFeatureFlagLoader
{
    Task LoadFeatureFlags(CancellationToken cancellationToken);
}

internal class FeatureFlagLoader(IGrapioServerGateway serverGateway, GrapioConfiguration configuration, IFeatureFlagsRepository repository, ILogger<FeatureFlagLoader> logger) : IFeatureFlagLoader
{
    public async Task LoadFeatureFlags(CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(serverGateway, nameof(serverGateway));
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        if (configuration.Offline)
        {
            logger.LogWarning("Provider is running as Offline. Skipping the loading of feature flags from the Grapio Server.");
            return;
        }

        logger.LogInformation("Loading feature flags from the Grapio Server");
        var featureFlags = await serverGateway.FetchFeatureFlags(cancellationToken).ConfigureAwait(false);
        
        logger.LogInformation("Saving feature flags from the Grapio Server");
        await repository.SaveFeatureFlags(featureFlags, cancellationToken).ConfigureAwait(false);
    }
}
