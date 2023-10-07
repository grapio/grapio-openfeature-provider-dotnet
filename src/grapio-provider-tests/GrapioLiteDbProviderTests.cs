using grapio_common;
using grapio_provider;
using LiteDB;
using NSubstitute;
using OpenFeature.Constant;
using OpenFeature.Model;

namespace grapio_tests;

public class GrapioLiteDbProviderTests : IDisposable
{
    public class Customer
    {
        public ObjectId CustomerId { get; set; }
        public string Name { get; set; }
        public DateTime CreationDate { get; set; }
        public List<string> Phones { get; set; }
        public bool IsActive { get; set; }
    }
    
    private readonly Customer _customer;
    private readonly ILiteDatabase _database;
    private readonly GrapioLiteDbProvider _provider;

    public GrapioLiteDbProviderTests()
    {
        _database = new LiteDatabase("Filename=:temp:");
        _provider = new GrapioLiteDbProvider(_database);
        
        _customer = new Customer
        {
            Name = "Joe",
            Phones = new List<string>
            {
                "010555",
                "020555"
            },
            CreationDate = DateTime.Today,
            CustomerId = ObjectId.NewObjectId(),
            IsActive = true
        };
    }
    
    [Fact]
    public void Constructing_the_provider_must_accept_a_connection_string()
    {
        var exception = Record.Exception(() => 
            new GrapioLiteDbProvider("Filename=:temp:;Password=1234;Connection=direct;InitialSize=10MB;ReadOnly=false;Upgrade=false;")
        );
        
        Assert.Null(exception);
    }

    [Fact]
    public void Constructing_the_provider_with_an_empty_connection_string_must_throw_an_exception()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => new GrapioLiteDbProvider(""));
        Assert.Equal("Value cannot be null. (Parameter 'connectionString')", exception.Message);
    }
    
    [Fact]
    public void Constructing_the_provider_with_a_null_connection_string_must_throw_an_exception()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => new GrapioLiteDbProvider((string)null!));
        Assert.Equal("Value cannot be null. (Parameter 'connectionString')", exception.Message);
    }

    [Fact]
    public void Calling_get_metadata_must_return_metadata_with_the_name_of_the_provider()
    {
        using var provider = new GrapioLiteDbProvider(_database);
        Assert.Equal("GrapioProvider", provider.GetMetadata().Name);
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
        db.GetCollection<BooleanFeatureFlag>().Returns(x => throw new Exception("general failure"));
        
        using var provider = new GrapioLiteDbProvider(db);
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
        db.GetCollection<StringFeatureFlag>().Returns(x => throw new Exception("general failure"));
        
        using var provider = new GrapioLiteDbProvider(db);
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
        db.GetCollection<IntegerFeatureFlag>().Returns(x => throw new Exception("general failure"));
        
        using var provider = new GrapioLiteDbProvider(db);
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
        db.GetCollection<DoubleFeatureFlag>().Returns(x => throw new Exception("general failure"));
        
        using var provider = new GrapioLiteDbProvider(db);
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
        var serializedCustomer = System.Text.Json.JsonSerializer.Serialize(_customer);
        
        var resolution = await _provider.ResolveStructureValue("", new Value(serializedCustomer));
        Assert.Equal(ErrorType.General, resolution.ErrorType);
        Assert.Equal("Flag key is blank or null", resolution.Reason);
        Assert.Equal("Invalid flag key", resolution.ErrorMessage);
        Assert.Equal(serializedCustomer, resolution.Value.AsString);
    }
    
    [Fact]
    public async Task Calling_resolve_structure_value_with_a_non_existent_flag_key_should_return_flag_not_found_resolution_with_default_value()
    {
        var serializedCustomer = System.Text.Json.JsonSerializer.Serialize(_customer);
        
        var resolution = await _provider.ResolveStructureValue("flag", new Value(serializedCustomer));
        Assert.Equal(ErrorType.FlagNotFound, resolution.ErrorType);
        Assert.Equal(serializedCustomer, resolution.Value.AsString);
    }
    
    [Fact]
    public async Task Calling_resolve_structure_value_and_an_exception_occurs_should_return_general_error_resolution_with_default_value()
    {
        var db = Substitute.For<ILiteDatabase>();
        db.GetCollection<ValueFeatureFlag>().Returns(x => throw new Exception("general failure"));
        
        using var provider = new GrapioLiteDbProvider(db);
        var resolution = await provider.ResolveStructureValue("flag", null!);
        Assert.Equal(ErrorType.General, resolution.ErrorType);
        Assert.Equal("Exception", resolution.Reason);
        Assert.Equal("general failure", resolution.ErrorMessage);
        Assert.Null(resolution.Value);
    }

    [Fact]
    public async Task Calling_resolve_structure_value_must_return_the_resolution_details_with_a_valid_value()
    {
        var serializedCustomer = System.Text.Json.JsonSerializer.Serialize(_customer);
        
        var collection = _database.GetCollection<ValueFeatureFlag>();
        collection.Insert(new ValueFeatureFlag("flag-1", serializedCustomer));
        collection.EnsureIndex(x => x.FlagKey, unique: true);
        
        var resolution = await _provider.ResolveStructureValue("flag-1", null!);
        Assert.Equal(serializedCustomer, resolution.Value.AsString);
    }
    
    public void Dispose()
    {
        _database.Dispose();
        _provider.Dispose();
        GC.SuppressFinalize(this);
    }
}