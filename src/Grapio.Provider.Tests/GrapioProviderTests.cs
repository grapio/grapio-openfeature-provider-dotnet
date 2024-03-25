using Microsoft.Extensions.Logging;
using Moq;
using OpenFeature.Constant;
using OpenFeature.Model;

namespace Grapio.Provider.Tests;

public class GrapioProviderTests
{
    private readonly GrapioProvider _provider;
    private readonly Mock<ILogger<GrapioProvider>> _logger;
    private readonly Mock<IFeatureFlagLoader> _featureFlagLoader;
    private readonly Mock<IFeatureFlagsRepository> _featureFlagRepository;

    public GrapioProviderTests()
    {
        _featureFlagLoader = new Mock<IFeatureFlagLoader>();
        _featureFlagRepository = new Mock<IFeatureFlagsRepository>();
        _logger = new Mock<ILogger<GrapioProvider>>();
        _provider = new GrapioProvider(_featureFlagLoader.Object, _featureFlagRepository.Object, _logger.Object);
    }
    
    [Fact]
    public void GetMetadata_should_return_the_provider_name()
    {
        Assert.Equal("Grapio Provider", _provider.GetMetadata().Name);
    }
    
    [Fact]
    public void GetStatus_should_return_not_ready_before_call_to_initialize()
    {
        var provider = new GrapioProvider(_featureFlagLoader.Object, _featureFlagRepository.Object, _logger.Object);
        Assert.Equal(ProviderStatus.NotReady, provider.GetStatus());
    }

    [Fact]
    public async Task GetStatus_should_return_ready_after_call_to_initialize()
    {
        await _provider.Initialize(EvaluationContext.Empty);
        Assert.Equal(ProviderStatus.Ready, _provider.GetStatus());
    }

    [Fact]
    public async Task Initialize_must_load_the_feature_flags()
    {
        _featureFlagLoader.Setup(l => l.LoadFeatureFlags(It.IsAny<CancellationToken>())).Verifiable();
        await _provider.Initialize(EvaluationContext.Empty);
        _featureFlagLoader.Verify();
    }

    [Fact]
    public async Task ResolveBoolean_must_return_error_resolution_when_the_provider_is_not_ready()
    {
        var resolution = await _provider.ResolveBooleanValue("key", true);
        AssertProviderNotReady(resolution);
        Assert.True(resolution.Value);
    }
    
    [Fact]
    public async Task ResolveString_must_return_error_resolution_when_the_provider_is_not_ready()
    {
        var resolution = await _provider.ResolveStringValue("key", "default");
        AssertProviderNotReady(resolution);
        Assert.Equal("default", resolution.Value);
    }
    
    [Fact]
    public async Task ResolveInteger_must_return_error_resolution_when_the_provider_is_not_ready()
    {
        var resolution = await _provider.ResolveIntegerValue("key", 999);
        AssertProviderNotReady(resolution);
        Assert.Equal(999, resolution.Value);
    }
    
    [Fact]
    public async Task ResolveDouble_must_return_error_resolution_when_the_provider_is_not_ready()
    {
        var resolution = await _provider.ResolveDoubleValue("key", 3.142);
        AssertProviderNotReady(resolution);
        Assert.Equal(3.142, resolution.Value);
    }
    
    [Fact]
    public async Task ResolveStructure_must_return_error_resolution_when_the_provider_is_not_ready()
    {
        var defaultValue = new Value("json");
        var resolution = await _provider.ResolveStructureValue("key", defaultValue);
        AssertProviderNotReady(resolution);
        Assert.Equal(defaultValue, resolution.Value);
    }
    
    [Fact]
    public async Task ResolveBoolean_must_return_error_resolution_when_the_flag_is_not_found()
    {
        await _provider.Initialize(EvaluationContext.Empty);
        var resolution = await _provider.ResolveBooleanValue("key", true);
        AssertFlagNotFound(resolution);
        Assert.True(resolution.Value);
    }
    
    [Fact]
    public async Task ResolveString_must_return_error_resolution_when_the_flag_is_not_found()
    {
        await _provider.Initialize(EvaluationContext.Empty);
        var resolution = await _provider.ResolveStringValue("key", "default");
        AssertFlagNotFound(resolution);
        Assert.Equal("default", resolution.Value);
    }
    
    [Fact]
    public async Task ResolveInteger_must_return_error_resolution_when_the_flag_is_not_found()
    {
        await _provider.Initialize(EvaluationContext.Empty);
        var resolution = await _provider.ResolveIntegerValue("key", 988);
        AssertFlagNotFound(resolution);
        Assert.Equal(988, resolution.Value);
    }
    
    [Fact]
    public async Task ResolveDouble_must_return_error_resolution_when_the_flag_is_not_found()
    {
        await _provider.Initialize(EvaluationContext.Empty);
        var resolution = await _provider.ResolveDoubleValue("key", 3.142);
        AssertFlagNotFound(resolution);
        Assert.Equal(3.142, resolution.Value);
    }
    
    [Fact]
    public async Task ResolveStructure_must_return_error_resolution_when_the_flag_is_not_found()
    {
        await _provider.Initialize(EvaluationContext.Empty);
        var defaultValue = new Value("json");
        var resolution = await _provider.ResolveStructureValue("key", defaultValue);
        AssertFlagNotFound(resolution);
        Assert.Equal(defaultValue, resolution.Value);
    }
    
    [Fact]
    public async Task ResolveBoolean_must_return_flag_feature_value()
    {
        _featureFlagRepository.Setup(r => r.FetchFeatureFlag("key", CancellationToken.None))
            .ReturnsAsync(() => (true, new FeatureFlag("key", true)));
        
        await _provider.Initialize(EvaluationContext.Empty);
        var resolution = await _provider.ResolveBooleanValue("key", false);
        AssertCachedValue(resolution);
        Assert.True(resolution.Value);
    }
    
    [Fact]
    public async Task ResolveString_must_return_flag_feature_value()
    {
        _featureFlagRepository.Setup(r => r.FetchFeatureFlag("key", CancellationToken.None))
            .ReturnsAsync(() => (true, new FeatureFlag("key", "value-1")));
        
        await _provider.Initialize(EvaluationContext.Empty);
        var resolution = await _provider.ResolveStringValue("key", "default");
        AssertCachedValue(resolution);
        Assert.Equal("value-1", resolution.Value);
    }
    
    [Fact]
    public async Task ResolveInteger_must_return_flag_feature_value()
    {
        _featureFlagRepository.Setup(r => r.FetchFeatureFlag("key", CancellationToken.None))
            .ReturnsAsync(() => (true, new FeatureFlag("key", 999)));
        
        await _provider.Initialize(EvaluationContext.Empty);
        var resolution = await _provider.ResolveIntegerValue("key", 12);
        AssertCachedValue(resolution);
        Assert.Equal(999, resolution.Value);
    }
    
    [Fact]
    public async Task ResolveDouble_must_return_flag_feature_value()
    {
        _featureFlagRepository.Setup(r => r.FetchFeatureFlag("key", CancellationToken.None))
            .ReturnsAsync(() => (true, new FeatureFlag("key", 2.71828)));
        
        await _provider.Initialize(EvaluationContext.Empty);
        var resolution = await _provider.ResolveDoubleValue("key", 3.142);
        AssertCachedValue(resolution);
        Assert.Equal(2.71828, resolution.Value);
    }
    
    [Fact]
    public async Task ResolveStructure_must_return_flag_feature_value()
    {
        _featureFlagRepository.Setup(r => r.FetchFeatureFlag("key", CancellationToken.None))
            .ReturnsAsync(() => (true, new FeatureFlag("key", "json")));
        
        await _provider.Initialize(EvaluationContext.Empty);
        var resolution = await _provider.ResolveStructureValue("key", new Value("default"));
        AssertCachedValue(resolution);
        Assert.Equal("json", resolution.Value.AsString);
    }

    private static void AssertCachedValue<T>(ResolutionDetails<T> resolution)
    {
        Assert.Equal("key", resolution.FlagKey);
        Assert.Equal(ErrorType.None, resolution.ErrorType);
        Assert.Equal("CACHED", resolution.Reason);
    }

    private static void AssertDefault<T>(ResolutionDetails<T> resolution)
    {
        Assert.Equal("key", resolution.FlagKey);
        Assert.Equal("DEFAULT", resolution.Reason);
    }

    private static void AssertFlagNotFound<T>(ResolutionDetails<T> resolution)
    {
        Assert.Equal("ERROR", resolution.Reason);
        Assert.Equal("Flag key was not found in the database", resolution.ErrorMessage);
        Assert.Equal(ErrorType.FlagNotFound, resolution.ErrorType);
        Assert.Equal("key", resolution.FlagKey);
    }
    
    private static void AssertProviderNotReady<T>(ResolutionDetails<T> resolution)
    {
        Assert.Equal("ERROR", resolution.Reason);
        Assert.Equal("Grapio Provider is not ready", resolution.ErrorMessage);
        Assert.Equal(ErrorType.ProviderNotReady, resolution.ErrorType);
        Assert.Equal("key", resolution.FlagKey);
    }
}
