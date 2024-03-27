using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Grapio.Provider;

internal interface IGrapioServerConnection
{
    AsyncServerStreamingCall<FeatureFlagReply> FetchFeatureFlags();
}

internal class GrapioServerConnection(
    GrapioConfiguration configuration,
    GrapioService.GrapioServiceClient client, 
    ILogger<GrapioServerConnection> logger) : IGrapioServerConnection
{
    public AsyncServerStreamingCall<FeatureFlagReply> FetchFeatureFlags()
    {
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));
        ArgumentNullException.ThrowIfNull(client, nameof(client));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        
        try
        {
            logger.LogInformation("Loading feature flags from the Grapio Server for {requester}", configuration.Requester);
            
            var result = client.FetchFeatureFlags(new FeatureFlagsRequest
            {
                Requester = configuration.Requester
            });
            
            logger.LogDebug("Fetched feature flags from the Grapio Server");

            return result;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to fetch feature flags from Grapio Server");
            throw;
        }
    }
}
