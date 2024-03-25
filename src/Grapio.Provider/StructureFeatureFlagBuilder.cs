namespace Grapio.Provider;

public class StructureFeatureFlagBuilder: IFeatureFlagBuilder
{
    public bool Matches(FeatureFlagReply.ValueOneofCase toMatch)
    {
        return toMatch == FeatureFlagReply.ValueOneofCase.StructureValue;
    }

    public FeatureFlag Build(FeatureFlagReply featureFlag)
    {
        ArgumentNullException.ThrowIfNull(featureFlag, nameof(featureFlag));
        return new FeatureFlag(featureFlag.Key, featureFlag.StructureValue);
    }
}
