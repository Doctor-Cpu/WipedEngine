using Wiped.Shared.IoC;
using Wiped.Shared.Reflection;
using Wiped.Shared.VFS;

namespace Wiped.Shared.Lifecycle;

[AutoBind(typeof(ILifecycleManager))]
internal sealed class LifecycleManager : IManager, ILifecycleManager, IHotReloadable
{
	[Dependency] private readonly IReflectionManager _reflection = default!;

	private readonly Dictionary<Type, object> _instances = [];

	public void Shutdown()
	{
		_instances.Clear();
	}

	public T Get<T>() where T: notnull
		=> (T)Get(typeof(T));

	public object Get(Type type)
	{
		if (_instances.TryGetValue(type, out var existing))
			return existing;

#if DEBUG
		if (typeof(IManager).IsAssignableFrom(type))
			throw new InvalidOperationException($"Tried to fetch an instance of a manager rather than the interface for {type.FullName}");
#endif

		// prefer ioc
		if (IoCManager.TryResolve(type, out var resolved))
		{
			_instances[type] = resolved;
			return resolved;
		}

		var instance = Activator.CreateInstance(type)
			?? throw new InvalidOperationException($"Failed to create {type.FullName}");

		IoCManager.ResolveDependencies(instance);

		_instances[type] = instance;
		return instance;
	}

    public IEnumerable<T> GetAll<T>()
	{
		// ioc owned
		foreach (var instance in IoCManager.GetAllResolved())
		{
			if (instance is T t)
			{
				_instances[instance.GetType()] = instance;
				yield return t;
			}
		}

		// life cycle owned
		var types = _reflection.GetAllDerivedTypes<T>();
		foreach (var type in types)
		{
			// already handled
			if (typeof(IManager).IsAssignableFrom(type))
				continue;

			yield return (T)Get(type);
		}
	}
}
