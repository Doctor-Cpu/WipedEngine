namespace Wiped.Shared.Reflection;

internal sealed class TypeRegistry : ITypeRegistry
{
	private readonly Dictionary<Type, List<(Type type, ReflectionVisibility visibility)>> _derived = [];
    private readonly Dictionary<Type, List<(Type type, object attr, ReflectionVisibility visibility)>> _typeAttributes = [];

    public void RegisterDerived<TBase, TConcrete>(ReflectionVisibility visibility)
		where TBase : notnull
		where TConcrete : notnull

	{
		var baseType = typeof(TBase);
		var concreteType = typeof(TConcrete);

		if (!_derived.TryGetValue(baseType, out var list))
			_derived[baseType] = list = [];

		list.Add((concreteType, visibility));
	}

	public void RegisterAttribute<TType, TAttribute>(TAttribute attribute, ReflectionVisibility visibility)
		where TType : notnull
		where TAttribute : Attribute
	{
		var type = typeof(TType);
		var attrType = typeof(TAttribute);

		if (!_typeAttributes.TryGetValue(attrType, out var list))
			_typeAttributes[attrType] = list = [];

		list.Add((type, attribute, visibility));
	}

    public IEnumerable<Type> GetDerived<TBase>(ReflectionVisibility maxVisibility)
		where TBase : notnull
	{
		if (!_derived.TryGetValue(typeof(TBase), out var list))
			yield break;

		foreach (var (type, visibility) in list)
		{
			if (visibility <= maxVisibility)
				yield return type;
		}
	}

    public IEnumerable<KeyValuePair<Type, TAttribute>> GetTypesWithAttribute<TAttribute>(ReflectionVisibility maxVisibility)
		where TAttribute : Attribute
	{
		if (!_typeAttributes.TryGetValue(typeof(TAttribute), out var list))
			yield break;

		foreach (var (type, attr, visibility) in list)
		{
			if (visibility <= maxVisibility)
				yield return KeyValuePair.Create(type, (TAttribute)attr);
		}
	}
}
