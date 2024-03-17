using System.Runtime.CompilerServices;
using Grapio.Common;
using LiteDB;
using OpenFeature;
using OpenFeature.Constant;
using OpenFeature.Model;

[assembly: InternalsVisibleTo("Grapio.Provider.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace Grapio.Provider;

/// <summary>
/// A LiteDB based self-contained OpenFeature provider that performs flag evaluations. 
/// </summary>
public class GrapioProvider: FeatureProvider, IDisposable
{
    private readonly Func<ILiteDatabase> _createDatabase;
    private ILiteDatabase? _database;
    
    /// <summary>
    /// Creates an instance of the <see href="GrapioProvider"/> class with a LiteDB connection string.   
    /// </summary>
    /// <param name="configuration">A <see cref="GrapioConfiguration"/> object that can be used to configure the Grapio Provider.</param>
    public GrapioProvider(IGrapioConfiguration configuration)
    {
        var config = configuration ?? throw new ArgumentNullException(nameof(configuration));
        
        config.Validate();
        _createDatabase = () => _database = new LiteDatabase(config.ConnectionString);
    }

    internal GrapioProvider(ILiteDatabase database)
    {
        _createDatabase = () => _database = database;
    }

    /// <inheritdoc />
    public override Metadata GetMetadata()
    {
        return new Metadata("Grapio Provider");
    }

    /// <inheritdoc />
    public override Task<ResolutionDetails<bool>> ResolveBooleanValue(string flagKey, bool defaultValue, EvaluationContext context = null!)
    {
        return ResolveValue<BooleanFeatureFlag, bool>(flagKey, defaultValue);
    }

    /// <inheritdoc />
    public override Task<ResolutionDetails<string>> ResolveStringValue(string flagKey, string defaultValue, EvaluationContext context = null!)
    {
        return ResolveValue<StringFeatureFlag, string>(flagKey, defaultValue);
    }

    /// <inheritdoc />
    public override Task<ResolutionDetails<int>> ResolveIntegerValue(string flagKey, int defaultValue, EvaluationContext context = null!)
    {
        return ResolveValue<IntegerFeatureFlag, int>(flagKey, defaultValue);
    }

    /// <inheritdoc />
    public override Task<ResolutionDetails<double>> ResolveDoubleValue(string flagKey, double defaultValue, EvaluationContext context = null!)
    {
        return ResolveValue<DoubleFeatureFlag, double>(flagKey, defaultValue);
    }

    /// <inheritdoc />
    public override async Task<ResolutionDetails<Value>> ResolveStructureValue(string flagKey, Value defaultValue, EvaluationContext context = null!)
    {
        var resolution = await ResolveValue<ValueFeatureFlag, object>(flagKey, defaultValue);

        if (resolution.ErrorType == ErrorType.None)
        {
            return new ResolutionDetails<Value>(
                resolution.FlagKey,
                new Value(resolution.Value),
                resolution.ErrorType,
                resolution.Reason,
                resolution.Variant,
                resolution.ErrorMessage
            );
        }
        
        return new ResolutionDetails<Value>(
            resolution.FlagKey,
            defaultValue,
            resolution.ErrorType,
            resolution.Reason,
            resolution.Variant,
            resolution.ErrorMessage
        );
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _database?.Dispose();
        GC.SuppressFinalize(this);
    }
    
    private Task<ResolutionDetails<TValue>> ResolveValue<TFeat, TValue>(string flagKey, TValue defaultValue) where TFeat: FeatureFlagBase<TValue>
    {
        if (string.IsNullOrEmpty(flagKey))
        {
            var r = new ResolutionDetails<TValue>(flagKey, defaultValue, ErrorType.General, reason: "Flag key is blank or null", errorMessage: "Invalid flag key");
            return Task.FromResult(r);
        }

        try
        {
            _database = _createDatabase();
            
            var collection = _database.GetCollection<TFeat>();

            var exists = collection.Exists(f => f.FlagKey.Equals(flagKey, StringComparison.InvariantCultureIgnoreCase));
            if (exists)
            {
                var featureFlag = collection.FindOne(f => f.FlagKey.Equals(flagKey, StringComparison.InvariantCultureIgnoreCase));
                var r = new ResolutionDetails<TValue>(featureFlag.FlagKey, featureFlag.Value);
                return Task.FromResult(r);
            }

            var notFound = new ResolutionDetails<TValue>(flagKey, defaultValue, ErrorType.FlagNotFound);
            return Task.FromResult(notFound);
        }
        catch (Exception ex)
        {
            var error = new ResolutionDetails<TValue>(flagKey, defaultValue, ErrorType.General, errorMessage: ex.Message, reason: "Exception");
            return Task.FromResult(error);
        }
    }
}
