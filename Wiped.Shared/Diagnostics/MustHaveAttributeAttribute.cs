namespace Wiped.Shared.Diagnostics;

[AttributeUsage(AttributeTargets.GenericParameter, AllowMultiple = true)]
public sealed class MustHaveAttributeAttribute(params Type[] attrTypes) : Attribute
{
	public Type[] AttributeTypes{ get; } = attrTypes;
}
