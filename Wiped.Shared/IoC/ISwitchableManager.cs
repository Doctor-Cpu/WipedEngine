namespace Wiped.Shared.IoC;

public interface ISwitchableManager<TSelector> : IManager where TSelector : Enum
{
	static abstract TSelector Selector { get; }
}
