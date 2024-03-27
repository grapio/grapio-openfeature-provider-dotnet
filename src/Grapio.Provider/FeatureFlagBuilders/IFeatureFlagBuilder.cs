namespace Grapio.Provider.FeatureFlagBuilders;

internal interface IFeatureFlagBuilder
{
    bool Matches(FeatureFlagReply.ValueOneofCase toMatch);
    FeatureFlag Build(FeatureFlagReply featureFlag);
}
