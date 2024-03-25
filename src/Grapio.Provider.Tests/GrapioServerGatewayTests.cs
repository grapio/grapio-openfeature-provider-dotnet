using Grpc.Core;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Grapio.Provider.Tests;

public class GrapioServerGatewayTests
{
    private readonly IEnumerable<IFeatureFlagBuilder> _featureFlagBuilders = new IFeatureFlagBuilder[]
    {
        new BooleanFeatureFlagBuilder(),
        new StringFeatureFlagBuilder()
    };

    private readonly GrapioServerGateway _gateway;
    private readonly Mock<IGrapioServerConnection> _connection;

    public GrapioServerGatewayTests()
    {
        _connection = new Mock<IGrapioServerConnection>();
        _gateway = new GrapioServerGateway(_connection.Object, _featureFlagBuilders, NullLogger<GrapioServerGateway>.Instance);
    }
    
    [Fact]
    public async Task FetchFeatureFlags_should_return_the_converted_feature_flags()
    {
        var serverResponse = new []
        {
            new FeatureFlagReply { Key = "A", BooleanValue = true }, 
            new FeatureFlagReply { Key = "B", StringValue = "Hello, World!" }
        };
        
        var streamReader = new FeatureFlagAsyncStreamReader<FeatureFlagReply>(serverResponse);
        
        using var serverStreamingCall = new AsyncServerStreamingCall<FeatureFlagReply>(
            streamReader, 
            Task.FromResult(new Metadata()), 
            () => Status.DefaultSuccess, 
            () => [],
            () => {}
        );
        
        _connection.Setup(c => c.FetchFeatureFlags()).Returns(serverStreamingCall);
        
        var featureFlags = await _gateway.FetchFeatureFlags(CancellationToken.None);
        Assert.Contains(featureFlags, flag => flag.FlagKey == "A" && (bool)flag.Value);
        Assert.Contains(featureFlags, flag => flag.FlagKey == "B" && (string)flag.Value == "Hello, World!");
    }
    
    [Fact]
    public async Task FetchFeatureFlags_should_throw_an_exception_for_a_missing_feature_flag_builder()
    {
        var serverResponse = new []
        {
            new FeatureFlagReply { Key = "A" }
        };
        
        var streamReader = new FeatureFlagAsyncStreamReader<FeatureFlagReply>(serverResponse);
        
        using var serverStreamingCall = new AsyncServerStreamingCall<FeatureFlagReply>(
            streamReader, 
            Task.FromResult(new Metadata()), 
            () => Status.DefaultSuccess, 
            () => [],
            () => {}
        );
        
        _connection.Setup(c => c.FetchFeatureFlags()).Returns(serverStreamingCall);
        
        var exception = await Assert.ThrowsAsync<NotSupportedException>(() => _gateway.FetchFeatureFlags(CancellationToken.None));
        Assert.Equal("Missing a builder for None", exception.Message);
    }

    [Fact]
    public async void FetchFeatureFlags_should_throw_an_exception_for_not_ok_status()
    {
        using var serverStreamingCall = new AsyncServerStreamingCall<FeatureFlagReply>(
            new FeatureFlagAsyncStreamReader<FeatureFlagReply>([]),
            Task.FromResult(new Metadata()),  
            () => new Status(StatusCode.DataLoss, "Error"), 
            () => [],
            () => {}
        );
        
        _connection.Setup(c => c.FetchFeatureFlags()).Returns(serverStreamingCall);
        var exception = await Assert.ThrowsAsync<Exception>(() => _gateway.FetchFeatureFlags(CancellationToken.None));
        
        Assert.Equal("Error fetching feature flags from the Grapio Server. See the logs for more information.", exception.Message);
    }
}