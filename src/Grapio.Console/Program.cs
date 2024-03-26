using Grapio.Provider;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenFeature.Model;

var host = Host.CreateApplicationBuilder();

host.Services.AddGrapio(config =>
{
    config.ConnectionString = "Data Source=grapio.db";
    config.Requester = "Grapio.Console";
    config.ServerUri = new Uri("http://localhost:5231");
    config.Offline = false;
});

host.Logging.AddSimpleConsole(options =>
{
    options.SingleLine = true;
    options.TimestampFormat = "HH:mm:ss.ff ";
});

host.Configuration.AddJsonFile("appsettings.json");

var app = host.Build();

var provider = app.Services.GetRequiredService<GrapioProvider>();
await provider.Initialize(EvaluationContext.Empty);
var resolution = await provider.ResolveIntegerValue("myval", 1);
Console.WriteLine($"{resolution.FlagKey}={resolution.Value}");