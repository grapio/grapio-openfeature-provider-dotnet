using System.Data;
using System.Data.Common;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace Grapio.Provider;

public interface IFeatureFlagsRepository
{
    Task SaveFeatureFlags(IEnumerable<FeatureFlag> featureFlags, CancellationToken cancellationToken = default);
}

public class FeatureFlagsRepository(GrapioConfiguration configuration, ILogger<FeatureFlagsRepository> logger) : IFeatureFlagsRepository
{
    private async Task EnsureDatabaseTables()
    {
        await using var connection = new SqliteConnection(configuration.ConnectionString);

        try
        {
            await connection.OpenAsync();
            
            var tableExists = await FeatureFlagTableExists(connection);
            if (tableExists)
            {
                logger.LogDebug("FeatureFlag table exists in the database");
                return;
            }

            logger.LogInformation("Creating FeatureFlags database table");
            await CreateFeatureFlagTable(connection);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error creating database tables");
            throw;
        }
        finally
        {
            if(connection.State == ConnectionState.Open)
                await connection.CloseAsync();
        }
    }

    private async Task CreateFeatureFlagTable(IDbConnection connection)
    {
        var success = await connection.ExecuteAsync(
            "CREATE TABLE IF NOT EXISTS FeatureFlags(" +
            "FlagKey TEXT PRIMARY KEY, " +
            "Value BLOB NOT NULL" +
            ") WITHOUT ROWID;"
        );
            
        if (success != 0)
        {
            logger.LogError("Failed to create FeatureFlags table");
            throw new Exception("Failed to create feature flags table in database");
        }
    }

    private static async Task<bool> FeatureFlagTableExists(IDbConnection connection)
    {
        var result = await connection.QuerySingleAsync<int>(
            sql: "SELECT 1 FROM sqlite_master WHERE type='table' AND name='FeatureFlags';", 
            CommandType.Text);

        return result == 1; // table exists
    }

    public async Task SaveFeatureFlags(IEnumerable<FeatureFlag> featureFlags, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        ArgumentNullException.ThrowIfNull(featureFlags);
        
        logger.LogInformation("Checking the database for tables");
        await EnsureDatabaseTables();

        logger.LogInformation("Saving feature flags into the database");
        await InsertFeatureFlags(featureFlags, cancellationToken);
    }

    private async Task InsertFeatureFlags(IEnumerable<FeatureFlag> featureFlags, CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection(configuration.ConnectionString);

        try
        {
            await connection.OpenAsync(cancellationToken);
            await using var transaction = connection.BeginTransaction();
            
            logger.LogWarning("Deleting all existing records from FeatureFlags database table");
            await connection.DeleteAllAsync<FeatureFlag>();
        
            logger.LogWarning("Inserting feature flags into the database");
            await InsertFeatureFlag(featureFlags, transaction, connection, cancellationToken);
        
            logger.LogInformation("Committing save feature flags transaction");
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to save feature flags");
            throw;
        }
        finally
        {
            if(connection.State == ConnectionState.Open)
                await connection.CloseAsync();
        }
    }

    private async Task InsertFeatureFlag(
        IEnumerable<FeatureFlag> featureFlags, 
        DbTransaction transaction, 
        DbConnection connection, 
        CancellationToken cancellationToken)
    {
        foreach (var featureFlag in featureFlags)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                logger.LogInformation("Rolling back save feature flags transaction, cancellation was requested");
                await transaction.RollbackAsync(cancellationToken);
                await connection.CloseAsync();
                break;
            }
            
            logger.LogDebug("> Inserting [{key}]", featureFlag.FlagKey);
            await connection.InsertAsync(featureFlag, transaction);
        }
    }
}