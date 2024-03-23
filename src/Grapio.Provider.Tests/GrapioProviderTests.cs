namespace Grapio.Provider.Tests;

public class GrapioProviderTests
{
    [Fact]
    public void GetMetadata_should_return_the_provider_name()
    {
        var provider = new GrapioProvider();
        Assert.Equal("Grapio Provider", provider.GetMetadata().Name);
    }
}
