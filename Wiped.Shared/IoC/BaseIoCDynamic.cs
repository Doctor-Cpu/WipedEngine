namespace Wiped.Shared.IoC;

// allows the underlying implementation to be changed without reassigning everything using it
public abstract class BaseIoCDynamic
{
	private IManager _current;

	internal IManager ResolveUntyped() => Volatile.Read(ref _current);

	internal abstract Type ManagerType { get; }

	internal void Swap(IManager next)
	{
		Interlocked.Exchange(ref _current, next);
	}

	internal BaseIoCDynamic(IManager initial)
	{
		_current = initial;
	}
}
