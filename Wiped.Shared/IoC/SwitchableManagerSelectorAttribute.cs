namespace Wiped.Shared.IoC;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class SwitchableManagerSelectorAttribute<TInter, TSelector>(TSelector selector) : Attribute
	where TInter : IManager
	where TSelector : Enum
{
	internal Type ManagerInterface = typeof(TInter);
	internal TSelector Selector = selector;
}
