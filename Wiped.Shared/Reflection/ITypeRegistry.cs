namespace Wiped.Shared.Reflection;

public interface ITypeRegistry
{
    void RegisterDerived<TBase, TConcrete>(ReflectionVisibility visibility)
		where TBase : notnull
		where TConcrete : notnull;

	void RegisterAttribute<TType, TAttribute>(TAttribute attribute, ReflectionVisibility visibility)
		where TType : notnull
		where TAttribute : Attribute;

    IEnumerable<Type> GetDerived<TBase>(ReflectionVisibility maxVisibility)
		where TBase : notnull;

    IEnumerable<KeyValuePair<Type, TAttribute>> GetTypesWithAttribute<TAttribute>(ReflectionVisibility maxVisibility)
		where TAttribute : Attribute;
}
