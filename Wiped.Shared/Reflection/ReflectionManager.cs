using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Wiped.Shared.IoC;

namespace Wiped.Shared.Reflection;

[AutoBind(typeof(IReflectionManager), typeof(IEngineReflectionManager))]
internal sealed class ReflectionManager : IReflectionManager, IEngineReflectionManager
{
	private ITypeRegistry _typeRegistry = default!;

	private const BindingFlags MemberAttributeFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

	public void ConsumeRegistry(IGeneratedModule root)
	{
		var registry = new TypeRegistry();
		ModuleRunner.RegisterReflection(root, registry);
		_typeRegistry = registry;
	}

	IEnumerable<Type> IReflectionManager.GetAllDerivedTypes<TBase>()
		=> GetAllDerivedTypes<TBase>(ReflectionVisibility.Content);
	IEnumerable<Type> IEngineReflectionManager.GetAllDerivedTypes<TBase>()
		=> GetAllDerivedTypes<TBase>(ReflectionVisibility.Engine);

	private IEnumerable<Type> GetAllDerivedTypes<TBase>(ReflectionVisibility maxVisibility) where TBase : notnull
	{
		var baseType = typeof(TBase);
		if (baseType.GetCustomAttribute<ReflectableBaseUsageAttribute>() is not { } )
			throw new InvalidOperationException($"{baseType} must have the [ReflectableBaseUsage] attribute in order to get derived types");

		foreach (var type in _typeRegistry.GetDerived<TBase>(maxVisibility))
			yield return type;
	}

	IEnumerable<KeyValuePair<Type, TAttribute>> IReflectionManager.GetTypesWithAttribute<TAttribute>(bool allowUnimplementedTypes)
		=> GetTypesWithAttribute<TAttribute>(allowUnimplementedTypes, ReflectionVisibility.Content);
	IEnumerable<KeyValuePair<Type, TAttribute>> IEngineReflectionManager.GetTypesWithAttribute<TAttribute>(bool allowUnimplementedTypes)
		=> GetTypesWithAttribute<TAttribute>(allowUnimplementedTypes, ReflectionVisibility.Engine);

	private IEnumerable<KeyValuePair<Type, TAttribute>> GetTypesWithAttribute<TAttribute>(bool allowUnimplementedTypes, ReflectionVisibility maxVisibility) where TAttribute : Attribute
	{
		var attrType = typeof(TAttribute);
		if (attrType.GetCustomAttribute<ReflectableAttributeUsageAttribute>() is not { } )
			throw new InvalidOperationException($"{attrType} must have the [ReflectableAttributeUsage] attribute in order to get derived types");

		foreach (var kv in _typeRegistry.GetTypesWithAttribute<TAttribute>(maxVisibility))
		{
			var (type, _) = kv;

			if (!allowUnimplementedTypes)
			{
				if (type.IsAbstract || type.IsInterface)
					continue;
			}

			yield return kv;
		}
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
                yield return new IReflectionManager.MemberAttributeInfo<TAttribute>(member.Name, member, memberType, attr);
		}
	}

	public bool TryGetStaticValue<T>(MemberInfo member, [NotNullWhen(true)] out T? value) where T : class
	{
		value = member switch
		{
			FieldInfo f when f.IsStatic => f.GetValue(null) as T,
			PropertyInfo p when p.GetMethod?.IsStatic == true => p.GetValue(null) as T,
			_ => null
		};

		return value != null;
	}
}
