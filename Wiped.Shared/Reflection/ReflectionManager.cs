using System.Reflection;
using Wiped.Shared.IoC;

namespace Wiped.Shared.Reflection;

internal sealed class ReflectionManager : BaseManager, IReflectionManager
{
	private readonly List<Assembly> _assemblies = [];
    private readonly Dictionary<Type, List<Type>> _derivedTypeCache = [];


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

	public ReflectionManager() : base()
	{
		_assemblies.AddRange(AppDomain.CurrentDomain.GetAssemblies());
	}
}
