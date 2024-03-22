using System.Diagnostics;
using System.Runtime.CompilerServices;
using Grapio.Common;
using Grpc.Core;
using Grpc.Net.Client;
using LiteDB;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OpenFeature;
using OpenFeature.Constant;
using OpenFeature.Model;
using Metadata = OpenFeature.Model.Metadata;

[assembly: InternalsVisibleTo("Grapio.Provider.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace Grapio.Provider;

/// <summary>
/// A LiteDB based self-contained OpenFeature provider that performs flag evaluations. 
/// </summary>
public class GrapioProvider: FeatureProvider, IDisposable
{
    private readonly ILogger<GrapioProvider> _logger;
    private readonly IGrapioConfiguration _config;
    private ILiteDatabase _database = null!;
    private ProviderStatus _status;

    /// <summary>
    /// Creates an instance of the <see href="GrapioProvider"/> class.   
    /// </summary>
    /// <param name="configuration">A <see cref="GrapioConfiguration"/> object that can be used to configure the Grapio Provider.</param>
    /// <param name="logger">Instance of <see cref="ILogger{TCategoryName}"/>.</param>
    public GrapioProvider(IGrapioConfiguration configuration, ILogger<GrapioProvider> logger)
    {
        _config = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        logger.LogInformation("Grapio Provider is starting...");
        _status = ProviderStatus.NotReady;
    }

    internal GrapioProvider(ILiteDatabase database)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
        _config = GrapioConfiguration.Default;
        _logger = NullLogger<GrapioProvider>.Instance;
        _status = ProviderStatus.Ready;
    }

    /// <inheritdoc />
    public override async Task Initialize(EvaluationContext context)
    {
        if (_status == ProviderStatus.Ready)
            return;
        
        _database = new LiteDatabase(_config.ConnectionString);

        if (_config.LoadFeatureFlagsFromServer)
            await LoadFeatureFlagsFromServer();

        _status = ProviderStatus.Ready;
        _logger.LogInformation("Grapio Provider is initialized.");
    }

    private async Task LoadFeatureFlagsFromServer()
    {
        try
        {
            using var channel = GrpcChannel.ForAddress(_config.ServerAddress);
            var client = new FeatureFlags.FeatureFlagsClient(channel);
            var reply = client.FetchFlags(new FeatureFlagsRequest { Requester = "GrapioProvider" });

            while (await reply.ResponseStream.MoveNext(new CancellationToken()))
            {
                var current = reply.ResponseStream.Current;

                switch (current.ValueCase)
                {
                    case FeatureFlagReply.ValueOneofCase.None:
                        break;
                    case FeatureFlagReply.ValueOneofCase.BooleanValue:
                        _logger.LogDebug($"Loading feature flag: {current.Name} = {current.BooleanValue}");        
                        break;
                    case FeatureFlagReply.ValueOneofCase.StringValue:
                        _logger.LogDebug($"Loading feature flag: {current.Name} = {current.StringValue}");
                        break;
                    case FeatureFlagReply.ValueOneofCase.IntegerValue:
                        _logger.LogDebug($"Loading feature flag: {current.Name} = {current.IntegerValue}");
                        break;
                    case FeatureFlagReply.ValueOneofCase.DoubleValue:
                        _logger.LogDebug($"Loading feature flag: {current.Name} = {current.DoubleValue}");
                        break;
                    case FeatureFlagReply.ValueOneofCase.StructureValue:
                        _logger.LogDebug($"Loading feature flag: {current.Name} = {current.StructureValue}");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            await channel.ShutdownAsync();
        }
        catch (RpcException ex)
        {
            _status = ProviderStatus.Error;
            _logger.LogError(ex, $"RPC exception fetching feature flags from the Grapio Server: {{Code: {ex.StatusCode}, Status: {ex.Status.Detail}}}");
            throw;
        }
        catch (Exception ex)
        {
            _status = ProviderStatus.Error;
            _logger.LogError(ex, "Failed to load feature flags from the Grapio Server.");
            throw;
        }
    }

    /// <inheritdoc />
    public override ProviderStatus GetStatus()
    {
        return _status;
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
        _database.Dispose();
        GC.SuppressFinalize(this);
    }
    
    private Task<ResolutionDetails<TValue>> ResolveValue<TFeat, TValue>(string flagKey, TValue defaultValue) where TFeat: FeatureFlagBase<TValue>
    {
        if (_status != ProviderStatus.Ready)
        {
            throw new InvalidOperationException("The Grapio Provider is not ready yet or the Initialize method was not called.");
        }
        
        if (string.IsNullOrEmpty(flagKey))
        {
            _logger.LogError("No value was provided for flagKey");
            var r = new ResolutionDetails<TValue>(flagKey, defaultValue, ErrorType.General, reason: "Flag key is blank or null", errorMessage: "Invalid flag key");
            return Task.FromResult(r);
        }

        _logger.LogDebug($"Resolving value for flagKey '{flagKey}'");
        
        try
        {
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
