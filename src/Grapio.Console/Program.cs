using Grapio.Console;
using Grapio.Provider;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = Host.CreateApplicationBuilder(args);

host.Logging.AddConsole();

host.Configuration.AddJsonFile("appsettings.json");
host.Configuration.AddEnvironmentVariables();

var config = new GrapioConfiguration();
host.Configuration.GetSection("Grapio").Bind(config);

host.Services.AddGrapio(config);

host.Services.AddTransient<Test>();

var app = host.Build();

var test = app.Services.GetRequiredService<Test>();
await test.Greet();
