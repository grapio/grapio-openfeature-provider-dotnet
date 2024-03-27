namespace Grapio.Provider.FeatureFlagBuilders;

internal class StringFeatureFlagBuilder: IFeatureFlagBuilder
{
    public bool Matches(FeatureFlagReply.ValueOneofCase toMatch)
    {
        return toMatch == FeatureFlagReply.ValueOneofCase.StringValue;
    }

    public FeatureFlag Build(FeatureFlagReply featureFlag)
    {
        ArgumentNullException.ThrowIfNull(featureFlag, nameof(featureFlag));
        return new FeatureFlag(featureFlag.Key, featureFlag.StringValue);
    }
}
