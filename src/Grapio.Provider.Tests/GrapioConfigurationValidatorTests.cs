using FluentValidation.Results;
using FluentValidation.TestHelper;

namespace Grapio.Provider.Tests;

public class GrapioConfigurationValidatorTests
{
    [Fact]
    public void Validate_should_return_validation_error_for_an_empty_requester()
    {
        var config = new GrapioConfiguration
        {
            Offline = false
        };
            
        var validator = new GrapioConfigurationValidator();
        var validationResult = validator.Validate(config);
        var testValidationResult = new TestValidationResult<GrapioConfiguration>(validationResult);
        
        testValidationResult
            .ShouldHaveValidationErrorFor(x => x.Requester)
            .WithErrorMessage("A requester must be specified when Offline is false.");
    }
    
    [Fact]
    public void Validate_should_return_validation_error_for_an_invalid_connection_string()
    {
        var config = new GrapioConfiguration
        {
            Offline = false,
            ConnectionString = "DataSource=:memory:"
        };
            
        var validator = new GrapioConfigurationValidator();
        var validationResult = validator.Validate(config);
        var testValidationResult = new TestValidationResult<GrapioConfiguration>(validationResult);
        
        testValidationResult
            .ShouldHaveValidationErrorFor(x => x.ConnectionString)
            .WithErrorMessage("Connection string is invalid or contains :memory:");
    }
    
    [Fact]
    public void Validate_should_return_validation_error_for_too_small_refresh_interval()
    {
        var config = new GrapioConfiguration
        {
            Offline = false,
            Requester = "Grapio.Provider.Tests",
            RefreshInterval = 4
        };
            
        var validator = new GrapioConfigurationValidator();
        var validationResult = validator.Validate(config);
        var testValidationResult = new TestValidationResult<GrapioConfiguration>(validationResult);
        
        testValidationResult
            .ShouldHaveValidationErrorFor(x => x.RefreshInterval)
            .WithErrorMessage("'Refresh Interval' must be greater than or equal to '5'.");
    }
    
    [Fact]
    public void Validate_should_return_validation_error_for_too_large_refresh_interval()
    {
        var config = new GrapioConfiguration
        {
            Offline = false,
            Requester = "Grapio.Provider.Tests",
            RefreshInterval = 7201
        };
            
        var validator = new GrapioConfigurationValidator();
        var validationResult = validator.Validate(config);
        var testValidationResult = new TestValidationResult<GrapioConfiguration>(validationResult);
        
        testValidationResult
            .ShouldHaveValidationErrorFor(x => x.RefreshInterval)
            .WithErrorMessage("Refresh interval must be >= 5 and <= 7200 when Offline is false.");
    }
    
    [Fact]
    public void Validate_should_return_validation_error_for_invalid_server_uri()
    {
        var config = new GrapioConfiguration
        {
            Offline = false,
            ServerUri = "invalid"
        };
            
        var validator = new GrapioConfigurationValidator();
        var validationResult = validator.Validate(config);
        var testValidationResult = new TestValidationResult<GrapioConfiguration>(validationResult);
        
        testValidationResult
            .ShouldHaveValidationErrorFor(x => x.ServerUri)
            .WithErrorMessage("Server URI is invalid and Offline is false.");
    }
    
    [Fact]
    public void Validate_should_not_have_validation_errors_when_offline()
    {
        var config = new GrapioConfiguration
        {
            Offline = true,
            RefreshInterval = -99,
            ServerUri = "invalid",
            Requester = string.Empty
        };
            
        var validator = new GrapioConfigurationValidator();
        var validationResult = validator.Validate(config);
        var testValidationResult = new TestValidationResult<GrapioConfiguration>(validationResult);
        
        testValidationResult.ShouldNotHaveValidationErrorFor(x => x.RefreshInterval);
        testValidationResult.ShouldNotHaveValidationErrorFor(x => x.ServerUri);
        testValidationResult.ShouldNotHaveValidationErrorFor(x => x.Requester);
        testValidationResult.ShouldNotHaveValidationErrorFor(x => x.RefreshInterval);
    }
}