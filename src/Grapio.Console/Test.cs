using Grapio.Provider;
using OpenFeature.Model;

namespace Grapio.Console;

public class Test(GrapioProvider provider)
{
    private readonly GrapioProvider _provider = provider ?? throw new ArgumentNullException(nameof(provider));

    public async Task Greet()
    {
        await _provider.Initialize(EvaluationContext.Empty);
    }
}
