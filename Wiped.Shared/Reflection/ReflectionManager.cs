using System.Reflection;
using Wiped.Shared.IoC;
using Wiped.Shared.VFS;

namespace Wiped.Shared.Reflection;

[AutoBind(typeof(IReflectionManager))]
internal sealed class ReflectionManager : IReflectionManager, IHotReloadable
{
	private readonly List<Assembly> _assemblies = [];
    private readonly Dictionary<Type, List<Type>> _derivedTypeCache = [];
	private readonly Dictionary<Type, List<(Type, Attribute)>> _attributeTypeCache = [];

	private const BindingFlags MemberAttributeFlags = BindingFlags.Instance | BindingFlags.Public;

	public void Initialize()
	{
		_assemblies.AddRange(AppDomain.CurrentDomain.GetAssemblies());
	}

	public void Shutdown()
	{
		_assemblies.Clear();
		_derivedTypeCache.Clear();
		_attributeTypeCache.Clear();
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

		foreach (var type in GetTypes())
		{
			if (type.IsAbstract || type.IsInterface)
				continue;

			if (!baseType.IsAssignableFrom(type))
				continue;

			yield return type;
			cached.Add(type);
		}

		_derivedTypeCache[baseType] = cached;
	}

	public IEnumerable<KeyValuePair<Type, TAttribute>> GetTypesWithAttribute<TAttribute>(bool allowUnimplementedTypes = false) where TAttribute : Attribute
	{
		var attrType = typeof(TAttribute);

		if (_attributeTypeCache.TryGetValue(attrType, out var cached))
		{
			foreach (var (type, attr) in cached)
				yield return KeyValuePair.Create(type, (TAttribute)attr);

			yield break;
		}

		cached = [];

		foreach (var type in GetTypes())
		{
			if (!allowUnimplementedTypes)
			{
				if (type.IsAbstract || type.IsInterface)
					continue;
			}

			if (type.GetCustomAttribute<TAttribute>() is not TAttribute attr)
				continue;

			yield return KeyValuePair.Create(type, attr);
			cached.Add((type, attr));
		}

		_attributeTypeCache[attrType] = cached;
	}

	public IEnumerable<IReflectionManager.MemberAttributeInfo<TAttribute>> GetMemberAttributes<TAttribute>(Type type) where TAttribute : Attribute
	{
		foreach (var member in type.GetMembers(MemberAttributeFlags))
		{
			if (member.GetCustomAttribute<TAttribute>() is not TAttribute attr)
				continue;

            var memberType = member switch
            {
                FieldInfo f => f.FieldType,
                PropertyInfo p => p.PropertyType,
                _ => null
            };

            if (memberType != null)
                yield return new IReflectionManager.MemberAttributeInfo<TAttribute>(member.Name, memberType, attr);
		}
	}

	private IEnumerable<Type> GetTypes()
	{
		foreach (var asm in _assemblies)
		{
			foreach (var type in SafeGetTypes(asm))
			{
				if (type is null)
					continue;

				yield return type;
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
