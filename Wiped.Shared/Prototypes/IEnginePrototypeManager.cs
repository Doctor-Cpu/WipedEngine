using System.Diagnostics.CodeAnalysis;
using Wiped.Shared.IoC;

namespace Wiped.Shared.Prototypes;

internal interface IEnginePrototypeManager : IManager
{
	void Register(IEnumerable<IPrototype> prototypes);

	void ResolveInheritance();

	bool TryGetWithAbstracts<T>(ProtoId<T> id, [NotNullWhen(true)] out T? prototype) where T: IPrototype;
}
