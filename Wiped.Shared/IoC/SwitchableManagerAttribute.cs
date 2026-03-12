namespace Wiped.Shared.IoC;

[AttributeUsage(AttributeTargets.Interface, AllowMultiple = true)]
public sealed class SwitchableManagerAttribute<TSelector>(TSelector defaultSelector) : Attribute where TSelector : Enum
{
	internal TSelector Default = defaultSelector;
}
