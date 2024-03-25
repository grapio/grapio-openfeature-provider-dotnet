using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging.Abstractions;

namespace Grapio.Provider.Tests;

public class FeatureFlagsRepositoryTests
{
    [Fact]
    public async Task SaveFeatureFlags_must_store_the_feature_flags_in_the_database()
    {
        var featureFlags = new[]
        {
            new FeatureFlag("key-1", "value-1"),
            new FeatureFlag("key-2", "value-2")
        };
        
        var configuration = new GrapioConfiguration
        {
            ConnectionString = "Data Source=grapio.db;Mode=ReadWriteCreate"
        };

        var repository = new FeatureFlagsRepository(configuration, NullLogger<FeatureFlagsRepository>.Instance);
        await repository.SaveFeatureFlags(featureFlags);

        await using var connection = new SqliteConnection("Data Source=grapio.db;Mode=ReadOnly");
        var result = connection.Query<FeatureFlag>("SELECT * FROM FeatureFlags");
        
        Assert.Equal(featureFlags, result);
    }
    
    [Fact]
    public async Task FetchFeatureFlag_must_return_the_feature_flag_from_the_database()
    {
        var featureFlags = new[]
        {
            new FeatureFlag("key-1", "value-1"),
            new FeatureFlag("key-2", "value-2")
        };
        
        var configuration = new GrapioConfiguration
        {
            ConnectionString = "Data Source=grapio.db;Mode=ReadWriteCreate"
        };

        var repository = new FeatureFlagsRepository(configuration, NullLogger<FeatureFlagsRepository>.Instance);
        var result = await repository.FetchFeatureFlag("key-2", CancellationToken.None);
        
        Assert.True(result.Found);
        Assert.Equal(new FeatureFlag("key-2", "value-2"), result.FeatureFlag);
    }
}