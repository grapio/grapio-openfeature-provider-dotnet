using LiteDB;
using OpenFeature.Model;

namespace grapio;

/// <summary>
/// Represents a <see cref="OpenFeature.Model.Value"/> feature flag.
/// </summary>
public class ValueFeatureFlag: FeatureFlagBase<object>
{
    /// <inheritdoc />
    public ValueFeatureFlag()
    {
    }
    
    /// <inheritdoc />
    [BsonCtor]
    public ValueFeatureFlag(string flagKey, object value) : base(flagKey, value)
    {
    }
}
