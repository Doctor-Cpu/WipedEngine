namespace Wiped.Shared.IoC;

public abstract class BaseManager
{
	protected internal virtual void Initialize()
	{
		IoCManager.ResolveDependencies(this);
	}

	protected internal virtual void Shutdown()
	{
	}
}
