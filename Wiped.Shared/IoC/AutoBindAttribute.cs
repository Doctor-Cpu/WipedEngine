namespace Wiped.Shared.IoC;

public sealed class AutoBindAttribute(params Type[] serviceTypes) : Attribute
{
	public Type[] ServiceTypes { get; } = serviceTypes;
}
