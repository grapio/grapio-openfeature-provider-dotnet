using Grapio.Provider.FeatureFlagBuilders;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Grapio.Provider;

internal interface IGrapioServerGateway
{
    Task<IEnumerable<FeatureFlag>> FetchFeatureFlags(CancellationToken cancellationToken);
}

internal class GrapioServerGateway(
    IGrapioServerConnection connection, 
    IEnumerable<IFeatureFlagBuilder> builders, 
    ILogger<GrapioServerGateway> logger) : IGrapioServerGateway
{
    public async Task<IEnumerable<FeatureFlag>> FetchFeatureFlags(CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(connection, nameof(connection));
        ArgumentNullException.ThrowIfNull(builders, nameof(builders));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        
        try
        {
            var serverResponse = connection.FetchFeatureFlags();
            return await BuildFeatureFlags(serverResponse, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error fetching feature flags from server. See the logs for more information.");
            throw;
        }
    }

    private async Task<IEnumerable<FeatureFlag>> BuildFeatureFlags(
        AsyncServerStreamingCall<FeatureFlagReply> serverResponse, 
        CancellationToken cancellationToken)
    {
        var featureFlags = new List<FeatureFlag>();
        var responseStream = serverResponse.ResponseStream;
        
        while (await responseStream.MoveNext(cancellationToken).ConfigureAwait(false))
        {
            var builder = FindFeatureFlagBuilder(responseStream.Current.ValueCase);
            var featureFlag = builder.Build(responseStream.Current);
            featureFlags.Add(featureFlag);
        }

        return featureFlags;
    }

    private IFeatureFlagBuilder FindFeatureFlagBuilder(FeatureFlagReply.ValueOneofCase featureFlagType)
    {
        foreach (var builder in builders)
            if (builder.Matches(featureFlagType)) return builder;
        
        logger.LogError("Missing a flag builder for [{type}]", featureFlagType);
        throw new NotSupportedException($"Missing a builder for {featureFlagType}");
    }
}
