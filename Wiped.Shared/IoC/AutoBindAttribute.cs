namespace Wiped.Shared.IoC;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class AutoBindAttribute(params Type[] serviceTypes) : Attribute
{
	public Type[] ServiceTypes { get; } = serviceTypes;
}
