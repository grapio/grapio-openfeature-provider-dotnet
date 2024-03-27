using System.Text;
using Google.Protobuf;

namespace Grapio.Provider.Tests;

public class FeatureFlagBuilderTests
{
    [Fact]
    public void Matches_should_return_true_for_boolean()
    {
        var builder = new BooleanFeatureFlagBuilder();
        Assert.True(builder.Matches(FeatureFlagReply.ValueOneofCase.BooleanValue));
    }
    
    [Fact]
    public void Build_should_return_a_boolean_feature_flag()
    {
        var builder = new BooleanFeatureFlagBuilder();
        var ff = builder.Build(new FeatureFlagReply
        {
            Key = "A", 
            BooleanValue = true
        });
        
        Assert.Equal("A", ff.FlagKey);
        Assert.True((bool)ff.Value!);
    }
    
    [Fact]
    public void Matches_should_return_true_for_string()
    {
        var builder = new StringFeatureFlagBuilder();
        Assert.True(builder.Matches(FeatureFlagReply.ValueOneofCase.StringValue));
    }
    
    [Fact]
    public void Build_should_return_a_string_feature_flag()
    {
        var builder = new StringFeatureFlagBuilder();
        var ff = builder.Build(new FeatureFlagReply
        {
            Key = "A", 
            StringValue = "Hello"
        });

        Assert.Equal("A", ff.FlagKey);
        Assert.Equal("Hello", ff.Value);
    }
    
    [Fact]
    public void Matches_should_return_true_for_integer()
    {
        var builder = new IntegerFeatureFlagBuilder();
        Assert.True(builder.Matches(FeatureFlagReply.ValueOneofCase.IntegerValue));
    }
    
    [Fact]
    public void Build_should_return_an_integer_feature_flag()
    {
        var builder = new IntegerFeatureFlagBuilder();
        var ff = builder.Build(new FeatureFlagReply
        {
            Key = "A", 
            IntegerValue = 2
        });
        
        Assert.Equal("A", ff.FlagKey);
        Assert.Equal(2, ff.Value);
    }
    
    [Fact]
    public void Matches_should_return_true_for_double()
    {
        var builder = new DoubleFeatureFlagBuilder();
        Assert.True(builder.Matches(FeatureFlagReply.ValueOneofCase.DoubleValue));
    }
    
    [Fact]
    public void Build_should_return_a_double_feature_flag()
    {
        var builder = new DoubleFeatureFlagBuilder();
        var ff = builder.Build(new FeatureFlagReply
        {
            Key = "A", 
            DoubleValue = 3.142
        });
        
        Assert.Equal("A", ff.FlagKey);
        Assert.Equal(3.142, ff.Value);
    }
    
    [Fact]
    public void Matches_should_return_true_for_structure()
    {
        var builder = new StructureFeatureFlagBuilder();
        Assert.True(builder.Matches(FeatureFlagReply.ValueOneofCase.StructureValue));
    }
    
    [Fact]
    public void Build_should_return_a_structure_feature_flag()
    {
        var builder = new StructureFeatureFlagBuilder();
        var ff = builder.Build(new FeatureFlagReply
        {
            Key = "A", 
            StructureValue = ByteString.CopyFromUtf8("Hello")
        });
        
        Assert.Equal("A", ff.FlagKey);
        Assert.Equal(ByteString.CopyFromUtf8("Hello"), ff.Value);
    }
}
