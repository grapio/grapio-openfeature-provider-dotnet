namespace Grapio.Provider.FeatureFlagBuilders;

internal class IntegerFeatureFlagBuilder: IFeatureFlagBuilder
{
    public bool Matches(FeatureFlagReply.ValueOneofCase toMatch)
    {
        return toMatch == FeatureFlagReply.ValueOneofCase.IntegerValue;
    }

    public FeatureFlag Build(FeatureFlagReply featureFlag)
    {
        ArgumentNullException.ThrowIfNull(featureFlag, nameof(featureFlag));
        return new FeatureFlag(featureFlag.Key, featureFlag.IntegerValue);
    }
}
