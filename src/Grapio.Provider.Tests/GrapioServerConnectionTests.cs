using Grpc.Core;
using Grpc.Core.Testing;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Grapio.Provider.Tests;

public class GrapioServerConnectionTests
{
    [Fact]
    public void FetchFeatureFlags_should_return_the_server_streaming_call()
    {
        var serverResponse = new[]
        {
            new FeatureFlagReply { Key = "A", BooleanValue = true },
            new FeatureFlagReply { Key = "B", StringValue = "Hello, World!" }
        };

        var streamReader = new FeatureFlagAsyncStreamReader<FeatureFlagReply>(serverResponse);

        var serverStreamingCall = TestCalls.AsyncServerStreamingCall(
            streamReader,
            new Task<Metadata>(() => new Metadata()),
            () => Status.DefaultSuccess,
            () => [], () => { }
        );

        var client = new Mock<GrapioService.GrapioServiceClient>();
        client.Setup(c => c.FetchFeatureFlags(It.IsAny<FeatureFlagsRequest>(), null, null, CancellationToken.None))
            .Returns(serverStreamingCall);

        var config = new GrapioConfiguration { Requester = "Grapio.Provider.Tests" };
        
        var connection = new GrapioServerConnection(config, client.Object, NullLogger<GrapioServerConnection>.Instance);
        var response = connection.FetchFeatureFlags();

        Assert.Equal(serverStreamingCall, response);
    }
}