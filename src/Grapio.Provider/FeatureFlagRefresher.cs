using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Grapio.Provider;

internal class FeatureFlagRefresher(GrapioConfiguration configuration, IFeatureFlagLoader featureFlagLoader, ILogger<FeatureFlagRefresher> logger): BackgroundService
{
    private static readonly Semaphore Semaphore = new(1, 1);
    
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(featureFlagLoader, nameof(featureFlagLoader));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        logger.LogInformation("Started feature flag refresher service");
        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        ArgumentNullException.ThrowIfNull(featureFlagLoader, nameof(featureFlagLoader));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                logger.LogDebug("Waiting for semaphore to refresh feature flags. Thread ID: [{threadId}]", Environment.CurrentManagedThreadId);
                Semaphore.WaitOne();

                logger.LogInformation("Refreshing feature flags from the Grapio Server: {time:o}", DateTimeOffset.Now);
                await featureFlagLoader.LoadFeatureFlags(stoppingToken);
                await Task.Delay(configuration.RefreshInterval * 1000, stoppingToken);
            }
            finally
            {
                logger.LogDebug("Releasing semaphore after refreshing feature flags. Thread ID: [{threadId}]", Environment.CurrentManagedThreadId);
                Semaphore.Release();
            }
        }
    }
}