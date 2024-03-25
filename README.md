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
The `GrapioProvider` is designed to fetch the feature flags from the [Grapio Server](https://github.com/grapio/grapio-server) and store them in an internal [SQLite](https://www.sqlite.org/) database on disk. The internal database location can be set via the connection string during the Grapio configuration (see the Configuration section below for more information).

Resolving the feature flag values happens directly from the database during runtime. In other words, access to the Grapio Server is only required to populate the internal SQLite database. Once the database has been populated, the provider no longer needs access to the Grapio Server.

The `GrapioProvider` can also be configured to run in an **offline** mode. In this mode, the internal database must be populated with the feature flags beforehand.

The advantage of populating the database with feature flags beforehand is that a container image can be built without needing access to the Grapio Server. However, the feature flags can still be changed at runtime via the [Grapio Control](https://github.com/grapio/grapio-ctl) command line utility.

**Note:** During the load process Grapio will delete all the feature flags from the internal database unless the provider is runnning as offline.

## Configuration
To configure the Grapio provider, use the extension method `AddGrapio()` which will register the `GrapioProvider` service and its configuration `GrapioConfiguration` as singletons. `AddGrapio()` will validate the configuration and throw a `ValidationException` if it fails. 

Below is an example configuration of the Grapio Provider:

```csharp
host.Services.AddGrapio(config =>
{
    config.ConnectionString = "Data Source=grapio.db;Mode=ReadWriteCreate";
    config.Requester = "GrapioProvider";
    config.ServerUri = new Uri("http://localhost:3278");
    config.Offline = true;
});
```

|Property|Description|Default|
|---|---|---|
|ConnectionString|Connection string to the internal database. `:memory:` is not supported.|`Data Source=grapio.db;Mode=ReadWriteCreate`|
|Requester|Name of the application or server requesting the feature flags. This is used to load a subset of feature flags.|`string.Empty`|
|ServerUri|Address of the Grapio Server.|http://localhost:3278|
|Offline|Set to `true` to load the feature flags from the Grapio Server at startup or, set to `false` to use the pre-populated database|false|

## Contributing
To get started, have a look at the [CONTRIBUTING](https://github.com/grapio/grapio-openfeature-provider-dotnet/blob/main/CONTRIBUTING.md) guide.
