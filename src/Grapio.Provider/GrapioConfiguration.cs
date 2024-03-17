using FluentValidation;
using FluentValidation.Results;

namespace Grapio.Provider;

/// <summary>
/// Grapio Provider configuration.
/// </summary>
public interface IGrapioConfiguration
{
    /// <summary>
    /// A LiteDB connection string to the configuration file. See https://www.litedb.org/docs/connection-string/
    /// </summary>
    string ConnectionString { get; set; }

    /// <summary>
    /// The Grapio ZeroMQ proxy address of the XPublisherSocket, for example, tcp://127.0.0.1:8652
    /// </summary>
    string ZmqProxyAddress { get; set; }

    /// <summary>
    /// Validates the properties of the <see cref="GrapioConfiguration"/> class
    /// </summary>
    /// <returns><see cref="ValidationResult"/></returns>
    void Validate();
}

/// <summary>
/// Grapio Provider configuration.
/// </summary>
public class GrapioConfiguration : IGrapioConfiguration
{
    /// <inheritdoc />
    public string ConnectionString { get; set; } = string.Empty;

    /// <inheritdoc />
    public string ZmqProxyAddress { get; set; } = string.Empty;

    /// <inheritdoc />
    public void Validate()
    {
        var validator = new GrapioConfigurationValidator();
        validator.ValidateAndThrow(this);
    }
}
