using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Wiped.Shared.IoC;

namespace Wiped.Shared.Reflection;

internal interface IEngineReflectionManager : IManager
{
	bool TryGetStaticValue<T>(MemberInfo member, [NotNullWhen(true)] out T? value) where T : class;
}
