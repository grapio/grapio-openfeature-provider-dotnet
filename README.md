![GitHub License](https://img.shields.io/github/license/grapio/grapio-openfeature-provider-dotnet)
![GitHub Issues or Pull Requests](https://img.shields.io/github/issues-pr/grapio/grapio-openfeature-provider-dotnet)
![GitHub Issues or Pull Requests](https://img.shields.io/github/issues/grapio/grapio-openfeature-provider-dotnet)
![GitHub Repo stars](https://img.shields.io/github/stars/grapio/grapio-openfeature-provider-dotnet)
![GitHub watchers](https://img.shields.io/github/watchers/grapio/grapio-openfeature-provider-dotnet)
![GitHub forks](https://img.shields.io/github/forks/grapio/grapio-openfeature-provider-dotnet)

# grapio-openfeature-provider-dotnet

## What is the Grapio OpenFeature Provider for .NET?
This OpenFeature provider is a provider that performs flag evaluations as specified by the [OpenFeature](https://openfeature.dev/) specification.

## Workings
The `GrapioProvider` is designed to store the feature flags using [FASTER KV](https://aka.ms/FASTER/), a fast concurrent persistent key-value store and log. This provider resolves the feature flag values directly from the key-value store. 

The database can either be populated with feature flags beforehand or it can load the feature flags and values, on startup, from the [Grapio Server](https://github.com/grapio/grapio-server). 

An advantage of populating the database with feature flags beforehand is that a container image can be built without needing access to the Grapio Server. However, the feature flags can still be changed at runtime via the [Grapio Control](https://github.com/grapio/grapio-ctl) command line utility.

## Configuration
To configure the Grapio provider, use the extension method `AddGrapio` which will register the `GrapioProvider` service and its configuration `GrapioConfiguration`  as singletons. `AddGrapio` will validate the configuration and throw a `ValidationException` if validation fails. The `GrapioProvider` can be configured to use LiteDB as an in-memory database using the following example configuration:

```csharp
builder.Services.AddGrapio(config =>
{
    config.ConnectionString = "Filename=:memory:";
});
```

Refer to the [LiteDB Connection String](https://www.litedb.org/docs/connection-string/) documentation for the different connection string options.

To pull the feature flags and values during startup, use the following example configuration:

```csharp
builder.Services.AddGrapio(config =>
{
    config.ConnectionString = "Filename=:memory:";
    config.LoadFeatureFlagsFromServer = true;
    config.ServerAddress = "https://example.com";
    config.RefreshInterval = 10;
});
```

If `LoadFeatureFlagsFromServer` is set to `true` then a `ServerAddress` must be specified.

A `RefreshInterval` can be specified, in seconds, which will reload the feature flags and their values from the Grapio Server.

The configuration may also be loaded from the `appsettings.json` file as shown below:

```csharp
var config = new GrapioConfiguration();
host.Configuration.GetSection("Grapio").Bind(config);
host.Services.AddGrapio(config);
```

## Contributing
To get started, have a look at the [CONTRIBUTING](https://github.com/grapio/grapio-openfeature-provider-dotnet/blob/main/CONTRIBUTING.md) guide.
