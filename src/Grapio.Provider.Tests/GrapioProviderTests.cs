using Grapio.Common;
using Grapio.Provider;
using LiteDB;
using NSubstitute;
using OpenFeature.Constant;
using OpenFeature.Model;

namespace Grapio.Tests;

public class GrapioProviderTests : IDisposable
{
    private class AppConfig
    {
        public ObjectId AppId = new();
        public string Name = string.Empty;
        public DateTime CreationDate; 
        public List<string> Ports = [];
        public bool IsActive;
    }
    
    private readonly AppConfig _appConfig;
    private readonly ILiteDatabase _database;
    private readonly GrapioProvider _provider;

    public GrapioProviderTests()
    {
        _database = new LiteDatabase("Filename=:memory:");
        _provider = new GrapioProvider(_database);
        
        _appConfig = new AppConfig
        {
            AppId = ObjectId.NewObjectId(),
            Name = "My Application",
            Ports = [
                "80",
                "443"
            ],
            CreationDate = DateTime.Today,
            IsActive = true
        };
    }
    
    [Fact]
    public void Calling_get_metadata_must_return_metadata_with_the_name_of_the_provider()
    {
        using var provider = new GrapioProvider(_database);
        Assert.Equal("Grapio Provider", provider.GetMetadata().Name);
    }

    [Fact]
    public async Task Calling_resolve_boolean_value_with_a_blank_flag_key_should_return_error_resolution_with_default_value()
    {
        var resolution = await _provider.ResolveBooleanValue("", false);
        Assert.Equal(ErrorType.General, resolution.ErrorType);
        Assert.Equal("Flag key is blank or null", resolution.Reason);
        Assert.Equal("Invalid flag key", resolution.ErrorMessage);
        Assert.False(resolution.Value);
    }
    
    [Fact]
    public async Task Calling_resolve_boolean_value_with_a_non_existent_flag_key_should_return_flag_not_found_resolution_with_default_value()
    {
        var resolution = await _provider.ResolveBooleanValue("flag", false);
        Assert.Equal(ErrorType.FlagNotFound, resolution.ErrorType);
        Assert.False(resolution.Value);
    }
    
    [Fact]
    public async Task Calling_resolve_boolean_value_and_an_exception_occurs_should_return_general_error_resolution_with_default_value()
    {
        var db = Substitute.For<ILiteDatabase>();
        db.GetCollection<BooleanFeatureFlag>().Returns(_ => throw new Exception("general failure"));
        
        using var provider = new GrapioProvider(db);
        var resolution = await provider.ResolveBooleanValue("flag", false);
        Assert.Equal(ErrorType.General, resolution.ErrorType);
        Assert.Equal("Exception", resolution.Reason);
        Assert.Equal("general failure", resolution.ErrorMessage);
        Assert.False(resolution.Value);
    }
    
    [Fact]
    public async Task Calling_resolve_boolean_value_must_return_the_resolution_details_with_a_valid_boolean_value()
    {
        var collection = _database.GetCollection<BooleanFeatureFlag>();
        collection.Insert(new BooleanFeatureFlag("flag-1", true));
        collection.EnsureIndex(x => x.FlagKey, unique: true);
        
        var flagValue = await _provider.ResolveBooleanValue("flag-1", false);
        Assert.True(flagValue.Value);
    }
    
    [Fact]
    public async Task Calling_resolve_string_value_with_a_blank_flag_key_should_return_error_resolution_with_default_value()
    {
        var resolution = await _provider.ResolveStringValue("", "default");
        Assert.Equal(ErrorType.General, resolution.ErrorType);
        Assert.Equal("Flag key is blank or null", resolution.Reason);
        Assert.Equal("Invalid flag key", resolution.ErrorMessage);
        Assert.Equal("default", resolution.Value);
    }
    
    [Fact]
    public async Task Calling_resolve_string_value_with_a_non_existent_flag_key_should_return_flag_not_found_resolution_with_default_value()
    {
        var resolution = await _provider.ResolveStringValue("flag", "default");
        Assert.Equal(ErrorType.FlagNotFound, resolution.ErrorType);
        Assert.Equal("default", resolution.Value);
    }
    
    [Fact]
    public async Task Calling_resolve_string_value_and_an_exception_occurs_should_return_general_error_resolution_with_default_value()
    {
        var db = Substitute.For<ILiteDatabase>();
        db.GetCollection<StringFeatureFlag>().Returns(_ => throw new Exception("general failure"));
        
        using var provider = new GrapioProvider(db);
        var resolution = await provider.ResolveStringValue("flag", "default");
        Assert.Equal(ErrorType.General, resolution.ErrorType);
        Assert.Equal("Exception", resolution.Reason);
        Assert.Equal("general failure", resolution.ErrorMessage);
        Assert.Equal("default", resolution.Value);
    }
    
    [Fact]
    public async Task Calling_resolve_string_value_must_return_the_resolution_details_with_a_valid_string_value()
    {
        var collection = _database.GetCollection<StringFeatureFlag>();
        collection.Insert(new StringFeatureFlag("flag-1", "flag-1-value"));
        collection.EnsureIndex(x => x.FlagKey, unique: true);
        
        var featureFlag = await _provider.ResolveStringValue("flag-1", "default");
        Assert.Equal("flag-1-value", featureFlag.Value);
    }
    
    [Fact]
    public async Task Calling_resolve_integer_value_with_a_blank_flag_key_should_return_error_resolution_with_default_value()
    {
        var resolution = await _provider.ResolveIntegerValue("", 222);
        Assert.Equal(ErrorType.General, resolution.ErrorType);
        Assert.Equal("Flag key is blank or null", resolution.Reason);
        Assert.Equal("Invalid flag key", resolution.ErrorMessage);
        Assert.Equal(222, resolution.Value);
    }
    
    [Fact]
    public async Task Calling_resolve_integer_value_with_a_non_existent_flag_key_should_return_flag_not_found_resolution_with_default_value()
    {
        var resolution = await _provider.ResolveIntegerValue("flag", 222);
        Assert.Equal(ErrorType.FlagNotFound, resolution.ErrorType);
        Assert.Equal(222, resolution.Value);
    }
    
    [Fact]
    public async Task Calling_resolve_integer_value_and_an_exception_occurs_should_return_general_error_resolution_with_default_value()
    {
        var db = Substitute.For<ILiteDatabase>();
        db.GetCollection<IntegerFeatureFlag>().Returns(_ => throw new Exception("general failure"));
        
        using var provider = new GrapioProvider(db);
        var resolution = await provider.ResolveIntegerValue("flag", 222);
        Assert.Equal(ErrorType.General, resolution.ErrorType);
        Assert.Equal("Exception", resolution.Reason);
        Assert.Equal("general failure", resolution.ErrorMessage);
        Assert.Equal(222, resolution.Value);
    }
    
    [Fact]
    public async Task Calling_resolve_integer_value_must_return_the_resolution_details_with_a_valid_value()
    {
        var collection = _database.GetCollection<IntegerFeatureFlag>();
        collection.Insert(new IntegerFeatureFlag("flag-1", 222));
        collection.EnsureIndex(x => x.FlagKey, unique: true);
        
        var flagValue = await _provider.ResolveIntegerValue("flag-1", 000);
        Assert.Equal(222, flagValue.Value);
    }
    
    [Fact]
    public async Task Calling_resolve_double_value_with_a_blank_flag_key_should_return_error_resolution_with_default_value()
    {
        var resolution = await _provider.ResolveDoubleValue("", 200.01);
        Assert.Equal(ErrorType.General, resolution.ErrorType);
        Assert.Equal("Flag key is blank or null", resolution.Reason);
        Assert.Equal("Invalid flag key", resolution.ErrorMessage);
        Assert.Equal(200.01, resolution.Value);
    }
    
    [Fact]
    public async Task Calling_resolve_double_value_with_a_non_existent_flag_key_should_return_flag_not_found_resolution_with_default_value()
    {
        var resolution = await _provider.ResolveDoubleValue("flag", 200.01);
        Assert.Equal(ErrorType.FlagNotFound, resolution.ErrorType);
        Assert.Equal(200.01, resolution.Value);
    }
    
    [Fact]
    public async Task Calling_resolve_double_value_and_an_exception_occurs_should_return_general_error_resolution_with_default_value()
    {
        var db = Substitute.For<ILiteDatabase>();
        db.GetCollection<DoubleFeatureFlag>().Returns(_ => throw new Exception("general failure"));
        
        using var provider = new GrapioProvider(db);
        var resolution = await provider.ResolveDoubleValue("flag", 200.01);
        Assert.Equal(ErrorType.General, resolution.ErrorType);
        Assert.Equal("Exception", resolution.Reason);
        Assert.Equal("general failure", resolution.ErrorMessage);
        Assert.Equal(200.01, resolution.Value);
    }
    
    [Fact]
    public async Task Calling_resolve_double_value_must_return_the_resolution_details_with_a_valid_value()
    {
        var collection = _database.GetCollection<DoubleFeatureFlag>();
        collection.Insert(new DoubleFeatureFlag("flag-1", 200.01));
        collection.EnsureIndex(x => x.FlagKey, unique: true);
        
        var flagValue = await _provider.ResolveDoubleValue("flag-1", 0.0);
        Assert.Equal(200.01, flagValue.Value);
    }
    
    [Fact]
    public async Task Calling_resolve_structure_value_with_a_blank_flag_key_should_return_error_resolution_with_default_value()
    {
        var appConfig = System.Text.Json.JsonSerializer.Serialize(_appConfig);
        
        var resolution = await _provider.ResolveStructureValue("", new Value(appConfig));
        Assert.Equal(ErrorType.General, resolution.ErrorType);
        Assert.Equal("Flag key is blank or null", resolution.Reason);
        Assert.Equal("Invalid flag key", resolution.ErrorMessage);
        Assert.Equal(appConfig, resolution.Value.AsString);
    }
    
    [Fact]
    public async Task Calling_resolve_structure_value_with_a_non_existent_flag_key_should_return_flag_not_found_resolution_with_default_value()
    {
        var appConfig = System.Text.Json.JsonSerializer.Serialize(_appConfig);
        
        var resolution = await _provider.ResolveStructureValue("flag", new Value(appConfig));
        Assert.Equal(ErrorType.FlagNotFound, resolution.ErrorType);
        Assert.Equal(appConfig, resolution.Value.AsString);
    }
    
    [Fact]
    public async Task Calling_resolve_structure_value_and_an_exception_occurs_should_return_general_error_resolution_with_default_value()
    {
        var db = Substitute.For<ILiteDatabase>();
        db.GetCollection<ValueFeatureFlag>().Returns(_ => throw new Exception("general failure"));
        
        using var provider = new GrapioProvider(db);
        var resolution = await provider.ResolveStructureValue("flag", null!);
        Assert.Equal(ErrorType.General, resolution.ErrorType);
        Assert.Equal("Exception", resolution.Reason);
        Assert.Equal("general failure", resolution.ErrorMessage);
        Assert.Null(resolution.Value);
    }

    [Fact]
    public async Task Calling_resolve_structure_value_must_return_the_resolution_details_with_a_valid_value()
    {
        var appConfig = System.Text.Json.JsonSerializer.Serialize(_appConfig);
        
        var collection = _database.GetCollection<ValueFeatureFlag>();
        collection.Insert(new ValueFeatureFlag("flag-1", appConfig));
        collection.EnsureIndex(x => x.FlagKey, unique: true);
        
        var resolution = await _provider.ResolveStructureValue("flag-1", null!);
        Assert.Equal(appConfig, resolution.Value.AsString);
    }
    
    public void Dispose()
    {
        _database.Dispose();
        _provider.Dispose();
        GC.SuppressFinalize(this);
    }
}