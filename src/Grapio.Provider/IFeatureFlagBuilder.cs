namespace Grapio.Provider;

public interface IFeatureFlagBuilder
{
    bool Matches(FeatureFlagReply.ValueOneofCase toMatch);
    FeatureFlag Build(FeatureFlagReply featureFlag);
}
