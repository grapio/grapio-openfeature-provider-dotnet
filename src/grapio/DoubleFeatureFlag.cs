namespace grapio;

/// <summary>
/// Represents a <see cref="System.Double"/> feature flag.
/// </summary>
public class DoubleFeatureFlag : FeatureFlagBase<double>
{
    /// <inheritdoc />
    public DoubleFeatureFlag(string flagKey, double value) : base(flagKey, value)
    {
    }
}   