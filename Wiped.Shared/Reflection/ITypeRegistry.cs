namespace Wiped.Shared.Reflection;

public interface ITypeRegistry
{
    void RegisterDerived(Type baseType, Type concreteType);
    IReadOnlyList<Type> GetDerived(Type baseType);
}
