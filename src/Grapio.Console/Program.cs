using Grapio.Provider;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenFeature.Model;

var host = Host.CreateApplicationBuilder();
host.Logging.AddSimpleConsole(options =>
{
    options.SingleLine = true;
    options.TimestampFormat = "HH:mm:ss.ff ";
});
host.Configuration.AddJsonFile("appsettings.json");
host.Services.AddSingleton<GrapioProvider>();

var app = host.Build();

var provider = app.Services.GetRequiredService<GrapioProvider>();
await provider.Initialize(EvaluationContext.Empty);

var loader = app.Services.GetRequiredService<FeatureFlagLoader>();
//await loader.FetchFeatureFlags();

// var resolution = await provider.ResolveBooleanValue("test", false, EvaluationContext.Empty);
// Console.WriteLine($"{resolution.FlagKey}={resolution.Value} [{resolution.ErrorType}]");

