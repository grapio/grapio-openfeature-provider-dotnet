namespace Grapio.Provider;

internal interface IFeatureFlagBuilder
{
    bool Matches(FeatureFlagReply.ValueOneofCase toMatch);
    FeatureFlag Build(FeatureFlagReply featureFlag);
}
