namespace Wiped.Shared.Serialization;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class DataFieldAttribute(string? name = null, bool required = false) : Attribute
{
    public string? Name { get; } = name;
	public bool Required { get; } = required;
}
