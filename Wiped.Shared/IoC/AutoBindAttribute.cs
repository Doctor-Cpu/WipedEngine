namespace Wiped.Shared.IoC;

[AttributeUsage(AttributeTargets.Class)]
public sealed class AutoBindAttribute(params Type[] serviceTypes) : Attribute
{
	public Type[] ServiceTypes { get; } = serviceTypes;
}
