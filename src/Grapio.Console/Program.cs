using Grapio.Provider;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = Host.CreateApplicationBuilder();

host.Services.AddGrapio(config =>
{
    config.ConnectionString = "DataSource=grapio.db;Mode=ReadWriteCreate;";
    config.Requester = "Grapio.Console";
    config.ServerUri = "http://localhost:5170";
    config.Offline = false;
    config.RefreshInterval = 1;
});

host.Logging.AddSimpleConsole(options =>
{
    options.SingleLine = true;
    options.TimestampFormat = "HH:mm:ss.ff ";
});

host.Configuration.AddJsonFile("appsettings.json");

var app = host.Build();

// var provider = app.Services.GetRequiredService<GrapioProvider>();
// await provider.Initialize(EvaluationContext.Empty);
// var resolution = await provider.ResolveIntegerValue("Key-4", 1);
// Console.WriteLine($"{resolution.FlagKey}={resolution.Value}");

await app.RunAsync();