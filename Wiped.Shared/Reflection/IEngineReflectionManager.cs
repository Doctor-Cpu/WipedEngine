using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Wiped.Shared.IoC;

namespace Wiped.Shared.Reflection;

internal interface IEngineReflectionManager : IManager
{
	void ConsumeRegistry(IGeneratedModule root);

	IEnumerable<Type> GetAllDerivedTypes<TBase>()
		where TBase : notnull;

	IEnumerable<KeyValuePair<Type, TAttribute>> GetTypesWithAttribute<TAttribute>(bool allowUnimplementedTypes = false)
		where TAttribute : Attribute;

	bool TryGetStaticValue<T>(MemberInfo member, [NotNullWhen(true)] out T? value) where T : class;
}
