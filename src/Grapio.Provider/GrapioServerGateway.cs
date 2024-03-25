using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace Grapio.Provider;

public interface IGrapioServerGateway
{
    Task<IEnumerable<FeatureFlag>> FetchFeatureFlags(CancellationToken cancellationToken);
}

public class GrapioServerGateway(
    IGrapioServerConnection connection, 
    IEnumerable<IFeatureFlagBuilder> builders, 
    ILogger<GrapioServerGateway> logger) : IGrapioServerGateway
{
    public async Task<IEnumerable<FeatureFlag>> FetchFeatureFlags(CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(connection, nameof(connection));
        ArgumentNullException.ThrowIfNull(builders, nameof(builders));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        
        var serverResponse = connection.FetchFeatureFlags();

        if (serverResponse.GetStatus().StatusCode == StatusCode.OK)
            return await BuildFeatureFlags(serverResponse, cancellationToken).ConfigureAwait(false);

        logger.LogError($"Error fetching feature flags from server. Status = [{serverResponse.GetStatus().StatusCode}]");
        throw new Exception("Error fetching feature flags from the Grapio Server. See the logs for more information.");
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
