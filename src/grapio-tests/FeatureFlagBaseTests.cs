using grapio;

namespace grapio_tests;

public class FeatureFlagBaseTests
{
    private class TestFeatureFlag : FeatureFlagBase<bool>
    {
        public TestFeatureFlag(string flagKey, bool value) : base(flagKey, value)
        {
        }
    }
    
    [Fact]
    public void When_constructing_a_feature_flag_with_an_empty_flag_key_it_should_throw_an_exception()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => new TestFeatureFlag("", true));
        Assert.Equal("Value cannot be null. (Parameter 'flagKey')", exception.Message);
    }
    
    [Fact]
    public void When_constructing_a_feature_flag_with_a_null_flag_key_it_should_throw_an_exception()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => new TestFeatureFlag(null!, true));
        Assert.Equal("Value cannot be null. (Parameter 'flagKey')", exception.Message);
    }
}