namespace Grapio.Provider;

internal class BooleanFeatureFlagBuilder: IFeatureFlagBuilder
{
    public bool Matches(FeatureFlagReply.ValueOneofCase toMatch)
    {
        return toMatch == FeatureFlagReply.ValueOneofCase.BooleanValue;
    }

    public FeatureFlag Build(FeatureFlagReply featureFlag)
    {
        ArgumentNullException.ThrowIfNull(featureFlag, nameof(featureFlag));
        return new FeatureFlag(featureFlag.Key, featureFlag.BooleanValue);
    }
}
