namespace Wiped.Shared.Reflection;

internal sealed class TypeRegistry : ITypeRegistry
{
	private readonly Dictionary<Type, List<Type>> _derived = [];

    public void RegisterDerived(Type baseType, Type concreteType)
	{
		if (!_derived.TryGetValue(baseType, out var list))
			_derived[baseType] = list = [];

		list.Add(concreteType);
	}

    public IReadOnlyList<Type> GetDerived(Type baseType)
		=> _derived.TryGetValue(baseType, out var list) ? list : [];
}
