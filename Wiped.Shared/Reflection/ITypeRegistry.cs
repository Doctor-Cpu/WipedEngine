namespace Wiped.Shared.Reflection;

public interface ITypeRegistry
{
    void RegisterDerived<TBase, TConcrete>(ReflectionVisibility visibility);
    IEnumerable<Type> GetDerived<TBase>(ReflectionVisibility maxVisibility);
}
