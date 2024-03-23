using OpenFeature.Model;

namespace Grapio.Provider;

public class GrapioProvider: OpenFeature.FeatureProvider
{
    public override Metadata GetMetadata()
    {
        return new Metadata("Grapio Provider");
    }

    public override Task<ResolutionDetails<bool>> ResolveBooleanValue(string flagKey, bool defaultValue, EvaluationContext context = null!)
    {
        throw new NotImplementedException();
    }

    public override Task<ResolutionDetails<string>> ResolveStringValue(string flagKey, string defaultValue, EvaluationContext context = null!)
    {
        throw new NotImplementedException();
    }

    public override Task<ResolutionDetails<int>> ResolveIntegerValue(string flagKey, int defaultValue, EvaluationContext context = null!)
    {
        throw new NotImplementedException();
    }

    public override Task<ResolutionDetails<double>> ResolveDoubleValue(string flagKey, double defaultValue, EvaluationContext context = null!)
    {
        throw new NotImplementedException();
    }

    public override Task<ResolutionDetails<Value>> ResolveStructureValue(string flagKey, Value defaultValue, EvaluationContext context = null!)
    {
        throw new NotImplementedException();
    }
}