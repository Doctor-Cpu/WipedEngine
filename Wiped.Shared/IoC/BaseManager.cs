namespace Wiped.Shared.IoC;

public abstract class BaseManager
{
	protected BaseManager()
	{
		IoCManager.ResolveDependencies(this);
	}
}
