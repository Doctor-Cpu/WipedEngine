using System.Diagnostics.CodeAnalysis;
using Wiped.Shared.IoC;

namespace Wiped.Shared.Prototypes;

public interface IPrototypeManager : IManager
{
	bool TryGet<T>(ProtoId<T> id, [NotNullWhen(true)] out T? prototype) where T: IPrototype;
	IEnumerable<T> GetAllOfType<T>() where T: IPrototype;
}
