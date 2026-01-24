namespace Wiped.Shared.Serialization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public sealed class DataDefinitionAttribute(string? name = null) : Attribute
{
	public string? Name { get; } = name;
}
