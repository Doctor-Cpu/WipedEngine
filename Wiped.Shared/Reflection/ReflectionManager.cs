using System.Reflection;
using Wiped.Shared.IoC;
using Wiped.Shared.Serialization;
using Wiped.Shared.VFS;

namespace Wiped.Shared.Reflection;

[AutoBind(typeof(IReflectionManager))]
internal sealed class ReflectionManager : IManager, IReflectionManager, IHotReloadable
{
	private readonly List<Assembly> _assemblies = [];
    private readonly Dictionary<Type, List<Type>> _derivedTypeCache = [];

	public Type[] Before => [typeof(DataDefinitionRegistryManager)];

	public void Initialize()
	{
		_assemblies.AddRange(AppDomain.CurrentDomain.GetAssemblies());
	}

	public void Shutdown()
	{
		_assemblies.Clear();
		_derivedTypeCache.Clear();
	}

	public IEnumerable<Type> GetAllDerivedTypes<TBase>()
	{
		var baseType = typeof(TBase);

		if (_derivedTypeCache.TryGetValue(baseType, out var cached))
		{
			foreach (var type in cached)
				yield return type;

			yield break;
		}

		cached = [];

		foreach (var asm in _assemblies)
		{
			foreach (var type in SafeGetTypes(asm))
			{
				if (type is null)
					continue;

				if (type.IsAbstract)
					continue;

				if (!baseType.IsAssignableFrom(type))
					continue;

				yield return type;
				cached.Add(type);
			}
		}
	}

    private static IEnumerable<Type> SafeGetTypes(Assembly assembly)
	{
		try
		{
			return assembly.GetTypes();
		}
		catch (ReflectionTypeLoadException e)
		{
			return e.Types.Where(t => t != null)!;
		}
	}
}
