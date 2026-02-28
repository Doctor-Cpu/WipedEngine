namespace Wiped.Shared.IoC;

internal sealed class IoCInstance(IManager instance)
{
	public IManager Instance = instance; 
}
