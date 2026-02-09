using Wiped.Shared.IoC;

namespace Wiped.Shared.Reflection;

public interface IReflectionManager : IManager
{
	IEnumerable<Type> GetAllDerivedTypes<TBase>();

	IEnumerable<KeyValuePair<Type, TAttribute>> GetTypesWithAttribute<TAttribute>(bool allowUnimplementedTypes = false) where TAttribute : Attribute;

	IEnumerable<MemberAttributeInfo<TAttribute>> GetMemberAttributes<TAttribute>(Type type) where TAttribute : Attribute;

	public readonly record struct MemberAttributeInfo<TAttribute>(string Name, Type Type, TAttribute Attribute) where TAttribute : Attribute;
}
