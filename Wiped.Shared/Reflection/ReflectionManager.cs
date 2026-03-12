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
	{
		var baseType = typeof(TBase);
		if (baseType.GetCustomAttribute<ReflectableAttribute>() is not { } )
			throw new InvalidOperationException($"{baseType} must have the [Reflectable] attribute in order to get derived types");

		foreach (var type in _typeRegistry.GetDerived<TBase>(ReflectionVisibility.Content))
			yield return type;
	}

	IEnumerable<Type> IEngineReflectionManager.GetAllDerivedTypes<TBase>()
	{
		var baseType = typeof(TBase);
		if (baseType.GetCustomAttribute<ReflectableAttribute>() is not { } )
			throw new InvalidOperationException($"{baseType} must have the [Reflectable] attribute in order to get derived types");

		foreach (var type in _typeRegistry.GetDerived<TBase>(ReflectionVisibility.Engine))
			yield return type;
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
