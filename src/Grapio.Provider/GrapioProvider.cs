using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using OpenFeature.Constant;
using OpenFeature.Model;

[assembly: InternalsVisibleTo("Grapio.Provider.Tests")]

namespace Grapio.Provider;

public class GrapioProvider(IFeatureFlagLoader featureFlagLoader, ILogger<GrapioProvider> logger): OpenFeature.FeatureProvider, IDisposable
{
    private ProviderStatus _status = ProviderStatus.NotReady;
    
    public override Metadata GetMetadata()
    {
        return new("Grapio Provider");
    }

    public override ProviderStatus GetStatus()
    {
        return _status;
    }

    public override async Task Initialize(EvaluationContext context)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(featureFlagLoader, nameof(featureFlagLoader));

        await featureFlagLoader.LoadFeatureFlags(new CancellationToken());
        
        _status = ProviderStatus.Ready;
        logger.LogInformation("Grapio Provider is initialized and ready.");
    }

    public override Task<ResolutionDetails<bool>> ResolveBooleanValue(string flagKey, bool defaultValue, EvaluationContext context = null!)
    {
        throw new NotImplementedException();
    }

    public override Task<ResolutionDetails<string>> ResolveStringValue(string flagKey, string defaultValue, EvaluationContext context = null!)
    {
        throw new NotImplementedException();
    }

    public override Task<ResolutionDetails<int>> ResolveIntegerValue(string flagKey, int defaultValue, EvaluationContext context = null!)
    {
        throw new NotImplementedException();
    }

    public override Task<ResolutionDetails<double>> ResolveDoubleValue(string flagKey, double defaultValue, EvaluationContext context = null!)
    {
        throw new NotImplementedException();
    }

    public override Task<ResolutionDetails<Value>> ResolveStructureValue(string flagKey, Value defaultValue, EvaluationContext context = null!)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
    }
}
