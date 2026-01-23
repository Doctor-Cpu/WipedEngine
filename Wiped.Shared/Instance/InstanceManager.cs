using Wiped.Shared.IoC;
using Wiped.Shared.Reflection;

namespace Wiped.Shared.Instance;

internal sealed class InstanceManager : BaseManager, IInstanceManager
{
	[Dependency] private readonly IReflectionManager _reflection = default!;

	private readonly Dictionary<Type, object> _instances = [];

	public T GetOrCreate<T>() where T : class
	{
		var type = typeof(T);
		return (T)GetOrCreate(type);
	}

	private object GetOrCreate(Type type)
	{
		if (_instances.TryGetValue(type, out var existing))
			return existing;

		var instance = Activator.CreateInstance(type)
            ?? throw new InvalidOperationException($"Failed to create {type.FullName}");

		_instances[type] = instance;
		return instance;
	}

	public IEnumerable<T> GetAll<T>() where T : class
	{
		var types = _reflection.GetAllDerivedTypes<T>();
		foreach (var type in types)
			yield return (T)GetOrCreate(type);
	}
}
