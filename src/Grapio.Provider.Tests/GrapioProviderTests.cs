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

    public GrapioProviderTests()
    {
        _featureFlagLoader = new Mock<IFeatureFlagLoader>();
        _logger = new Mock<ILogger<GrapioProvider>>();
        _provider = new GrapioProvider(_featureFlagLoader.Object, _logger.Object);
    }
    
    [Fact]
    public void GetMetadata_should_return_the_provider_name()
    {
        Assert.Equal("Grapio Provider", _provider.GetMetadata().Name);
    }
    
    [Fact]
    public void GetStatus_should_return_not_ready_before_call_to_initialize()
    {
        var provider = new GrapioProvider(_featureFlagLoader.Object, _logger.Object);
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
}
