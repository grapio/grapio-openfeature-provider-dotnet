using FluentValidation.TestHelper;
using Grapio.Provider;

namespace Grapio.Tests;

public class GrapioConfigurationValidatorTests
{
    private readonly GrapioConfiguration _configuration = new();

    [Fact]
    public void Validating_a_configuration_with_an_empty_connection_string_must_throw_an_exception()
    {
        _configuration.ConnectionString = string.Empty;
        
        var result = new GrapioConfigurationValidator().TestValidate(_configuration);
        
        result.ShouldHaveValidationErrorFor(prop => prop.ConnectionString)
            .WithErrorMessage("A valid LiteDb connection string must be set.");
    }
    
    [Fact]
    public void Validating_a_configuration_with_a_null_connection_string_must_throw_an_exception()
    {
        _configuration.ConnectionString = null!;

        var result = new GrapioConfigurationValidator().TestValidate(_configuration);
        
        result.ShouldHaveValidationErrorFor(prop => prop.ConnectionString)
            .WithErrorMessage("A valid LiteDb connection string must be set.");
    }
    
    [Fact]
    public void Validating_a_configuration_with_an_invalid_zeromq_proxy_address_must_throw_an_exception()
    {
        _configuration.ZmqProxyAddress = "invalid";

        var result = new GrapioConfigurationValidator().TestValidate(_configuration);
        
        result.ShouldHaveValidationErrorFor(prop => prop.ZmqProxyAddress)
            .WithErrorMessage("ZmqAddress must match the format <protocol>://<ip address>:<port>");
    }
    
    [Fact]
    public void Validating_a_configuration_with_an_unspecified_zeromq_proxy_address_must_not_throw_an_exception()
    {
        var result = new GrapioConfigurationValidator().TestValidate(_configuration);
        result.ShouldNotHaveValidationErrorFor(prop => prop.ZmqProxyAddress);
    }
}