using FluentValidation;
using FluentValidation.Results;

namespace Grapio.Provider;

/// <summary>
/// Grapio Provider configuration.
/// </summary>
public interface IGrapioConfiguration
{
    /// <summary>
    /// A LiteDB connection string to the configuration file. See https://www.litedb.org/docs/connection-string/.
    /// Defaults to Filename=:memory:.
    /// </summary>
    string ConnectionString { get; set; }
    
    /// <summary>
    /// Set this value to true if the provider must load the feature flags and values during startup from the
    /// Grapio Server. Default is false.
    /// </summary>
    bool LoadFeatureFlagsFromServer { get; set; }

    /// <summary>
    /// The Grapio Server address from which to fetch the feature flags and values. Default is http://localhost.
    /// </summary>
    string ServerAddress { get; set; }

    /// <summary>
    /// Refresh interval (between 5 and 7200 inclusive seconds) when the configuration will be refreshed
    /// from the Grapio Server. Default is 10 seconds.
    /// </summary>
    long RefreshInterval { get; set; }

    /// <summary>
    /// Validates the properties of the <see cref="GrapioConfiguration"/> class
    /// </summary>
    /// <returns><see cref="ValidationResult"/></returns>
    void ValidateAndThrow();
}

/// <inheritdoc />
public class GrapioConfiguration : IGrapioConfiguration
{
    /// <inheritdoc />
    public string ConnectionString { get; set; } = "Filename=:memory:";

    /// <inheritdoc />
    public bool LoadFeatureFlagsFromServer { get; set; } = false;

    /// <inheritdoc />
    public string ServerAddress { get; set; } = "http://localhost";

    /// <inheritdoc />
    public long RefreshInterval { get; set; } = 10;
    
    /// <inheritdoc />
    public void ValidateAndThrow()
    {
        var validator = new GrapioConfigurationValidator();
        validator.ValidateAndThrow(this);
    }

    /// <summary>
    /// Creates a new instance of the <see cref="GrapioConfiguration"/> and sets the properties to default values.
    /// </summary>
    /// <returns>A default Grapio configuration.</returns>
    public static GrapioConfiguration Default => new();
}
