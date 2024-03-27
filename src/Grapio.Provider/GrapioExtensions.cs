using FluentValidation;
using Grpc.Net.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Grapio.Provider;

public static class GrapioExtensions
{
    public static void AddGrapio(this IServiceCollection serviceCollection, Action<GrapioConfiguration> config)
    {
        var configuration = new GrapioConfiguration();
        config(configuration);

        new GrapioConfigurationValidator().ValidateAndThrow(configuration);

        serviceCollection.AddTransient<GrapioService.GrapioServiceClient>(
            _ => new GrapioService.GrapioServiceClient(GrpcChannel.ForAddress(configuration.ServerUri)));
        
        serviceCollection.AddSingleton(configuration);
        serviceCollection.AddTransient<IGrapioServerConnection, GrapioServerConnection>();
        serviceCollection.AddTransient<IGrapioServerGateway, GrapioServerGateway>();

        serviceCollection.AddTransient<IFeatureFlagBuilder, BooleanFeatureFlagBuilder>();
        serviceCollection.AddTransient<IFeatureFlagBuilder, StringFeatureFlagBuilder>();
        serviceCollection.AddTransient<IFeatureFlagBuilder, IntegerFeatureFlagBuilder>();
        serviceCollection.AddTransient<IFeatureFlagBuilder, DoubleFeatureFlagBuilder>();
        serviceCollection.AddTransient<IFeatureFlagBuilder, StructureFeatureFlagBuilder>();
        
        serviceCollection.AddTransient<IFeatureFlagLoader, FeatureFlagLoader>();
        serviceCollection.AddSingleton<IFeatureFlagsRepository, FeatureFlagsRepository>();
        serviceCollection.AddSingleton<GrapioProvider>();

        if (!configuration.Offline)
            serviceCollection.AddHostedService<FeatureFlagRefresher>();
    }
}