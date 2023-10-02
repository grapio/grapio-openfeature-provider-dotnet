namespace grapio;

/// <summary>
/// Represents an <see cref="System.Int32"/> feature flag.
/// </summary>
public class IntegerFeatureFlag : FeatureFlagBase<int>
{
    /// <inheritdoc />
    public IntegerFeatureFlag(string flagKey, int value) : base(flagKey, value)
    {
    }
}   