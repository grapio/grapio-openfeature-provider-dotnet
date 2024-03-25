using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Grapio.Provider.Tests;

public class FeatureFlagLoaderTests
{
    private readonly Mock<IGrapioServerGateway> _server;
    private readonly Mock<IFeatureFlagsRepository> _repository;
    private readonly FeatureFlagLoader _offlineProvider;
    private readonly FeatureFlagLoader _onlineProvider;

    public FeatureFlagLoaderTests()
    {
        _server = new Mock<IGrapioServerGateway>();
        _repository = new Mock<IFeatureFlagsRepository>();
        
        _offlineProvider = new FeatureFlagLoader(_server.Object, new GrapioConfiguration
        {
            Offline = true
        }, _repository.Object, NullLogger<FeatureFlagLoader>.Instance);
        
        _onlineProvider = new FeatureFlagLoader(_server.Object, new GrapioConfiguration
        {
            Offline = false
        }, _repository.Object, NullLogger<FeatureFlagLoader>.Instance);
    }
    
    [Fact]
    public async Task LoadFeatureFlags_should_not_call_the_grapio_server_or_repository_in_offline_mode()
    {
        await _offlineProvider.LoadFeatureFlags(CancellationToken.None);
        
        Assert.Empty(_server.Invocations);
        Assert.Empty(_repository.Invocations);
    }

    [Fact]
    public async Task LoadFeatureFlags_should_fetch_the_feature_flags_from_the_server_when_running_in_online_mode()
    {
        _server.Setup(s => s.FetchFeatureFlags(CancellationToken.None)).Verifiable();
        _repository.Setup(r => r.SaveFeatureFlags(It.IsAny<IEnumerable<FeatureFlag>>(), CancellationToken.None)).Verifiable();
        
        await _onlineProvider.LoadFeatureFlags(CancellationToken.None);
        
        _server.Verify();
        _repository.Verify();
    }
}