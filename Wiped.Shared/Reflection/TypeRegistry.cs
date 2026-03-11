namespace Wiped.Shared.Reflection;

internal sealed class TypeRegistry : ITypeRegistry
{
	private readonly Dictionary<Type, List<(Type type, ReflectionVisibility visibility)>> _derived = [];

    public void RegisterDerived<TBase, TConcrete>(ReflectionVisibility visibility)
	{
		var baseType = typeof(TBase);
		var concreteType = typeof(TConcrete);

		if (!_derived.TryGetValue(baseType, out var list))
			_derived[baseType] = list = [];

		list.Add((concreteType, visibility));
	}

    public IEnumerable<Type> GetDerived<TBase>(ReflectionVisibility maxVisibility)
	{
		if (!_derived.TryGetValue(typeof(TBase), out var list))
			yield break;

		foreach (var (type, visibility) in list)
		{
			if (visibility <= maxVisibility)
				yield return type;
		}
	}
}
