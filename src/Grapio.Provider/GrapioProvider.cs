using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using OpenFeature.Constant;
using OpenFeature.Model;

[assembly: InternalsVisibleTo("Grapio.Provider.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace Grapio.Provider;

public class GrapioProvider(
    IFeatureFlagLoader featureFlagLoader, 
    IFeatureFlagsRepository featureFlagsRepository, 
    ILogger<GrapioProvider> logger): OpenFeature.FeatureProvider
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

    public override async Task<ResolutionDetails<bool>> ResolveBooleanValue(string flagKey, bool defaultValue, EvaluationContext context = null!)
    {
        return await Resolve(flagKey, defaultValue, context, Convert.ToBoolean);
    }

    public override async Task<ResolutionDetails<string?>> ResolveStringValue(string flagKey, string defaultValue, EvaluationContext context = null!)
    {
        return await Resolve(flagKey, defaultValue, context, Convert.ToString);
    }

    public override async Task<ResolutionDetails<int>> ResolveIntegerValue(string flagKey, int defaultValue, EvaluationContext context = null!)
    {
        return await Resolve(flagKey, defaultValue, context, Convert.ToInt32);
    }

    public override async Task<ResolutionDetails<double>> ResolveDoubleValue(string flagKey, double defaultValue, EvaluationContext context = null!)
    {
        return await Resolve(flagKey, defaultValue, context, Convert.ToDouble);
    }

    public override async Task<ResolutionDetails<Value>> ResolveStructureValue(string flagKey, Value defaultValue, EvaluationContext context = null!)
    {
        return await Resolve(flagKey, defaultValue, context, o => new Value(o));
    }
    
    private async Task<ResolutionDetails<T>> Resolve<T>(string flagKey, T defaultValue, EvaluationContext context, Func<object, T> convert)
    {
        ArgumentException.ThrowIfNullOrEmpty(flagKey, nameof(flagKey));
        ArgumentNullException.ThrowIfNull(convert, nameof(convert));
        
        if (_status != ProviderStatus.Ready)
            return ProviderNotReady(flagKey, defaultValue);

        try
        {
            var (found, featureFlag) = await featureFlagsRepository.FetchFeatureFlag(flagKey);

            if (!found)
            {
                logger.LogWarning("Flag key [{key}] is not in the database", flagKey);
                return MissingFlag(flagKey, defaultValue);
            }

            var value = convert(featureFlag.Value ?? throw new InvalidOperationException("Feature flag value cannot be null"));
            return CachedFeatureFlag(flagKey, value);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Fetching feature flag [{0}] failed", flagKey);
            throw;
        }    
    }

    private static ResolutionDetails<T> CachedFeatureFlag<T>(string flagKey, T value)
    {
        return new ResolutionDetails<T>(
            flagKey,
            value, 
            ErrorType.None, 
            "CACHED", 
            flagMetadata: new FlagMetadata()
        );
    }

    private static ResolutionDetails<T> MissingFlag<T>(string flagKey, T defaultValue)
    {
        return new ResolutionDetails<T>(
            flagKey, 
            defaultValue,
            ErrorType.FlagNotFound,
            reason: "ERROR",
            errorMessage: "Flag key was not found in the database",
            flagMetadata: new FlagMetadata()
        );
    }

    private static ResolutionDetails<T> ProviderNotReady<T>(string flagKey, T defaultValue)
    {
        return new ResolutionDetails<T>(
            flagKey,
            defaultValue,
            ErrorType.ProviderNotReady,
            reason: "ERROR",
            errorMessage: "Grapio Provider is not ready",
            flagMetadata: new FlagMetadata()
        );
    }
}
