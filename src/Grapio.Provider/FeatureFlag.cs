namespace Grapio.Provider;

public class FeatureFlag: IEquatable<FeatureFlag>
{
    public string? FlagKey { get; }
    public object? Value { get; }

    public FeatureFlag()
    {
    }

    public static FeatureFlag Null = new NullFeatureFlag();

    private class NullFeatureFlag : FeatureFlag
    {
    }
    
    public FeatureFlag(string flagKey, object value)
    {
        ArgumentException.ThrowIfNullOrEmpty(flagKey, nameof(flagKey));
        ArgumentNullException.ThrowIfNull(value, nameof(value));
        
        FlagKey = flagKey;
        Value = value;
    }

    public bool Equals(FeatureFlag? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return FlagKey == other.FlagKey && Value.Equals(other.Value);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((FeatureFlag)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(FlagKey, Value);
    }
}