namespace Grapio.Provider.FeatureFlagBuilders;

internal class DoubleFeatureFlagBuilder: IFeatureFlagBuilder
{
    public bool Matches(FeatureFlagReply.ValueOneofCase toMatch)
    {
        return toMatch == FeatureFlagReply.ValueOneofCase.DoubleValue;
    }

    public FeatureFlag Build(FeatureFlagReply featureFlag)
    {
        ArgumentNullException.ThrowIfNull(featureFlag, nameof(featureFlag));
        return new FeatureFlag(featureFlag.Key, featureFlag.DoubleValue);
    }
}
