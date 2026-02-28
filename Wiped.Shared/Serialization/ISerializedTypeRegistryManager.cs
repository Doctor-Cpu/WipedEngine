using System.Diagnostics.CodeAnalysis;
using Wiped.Shared.IoC;
using Wiped.Shared.Serialization.Schema;

namespace Wiped.Shared.Serialization;

internal interface ISerializedTypeRegistryManager : IManager
{
	void Register(SerializedType type);
	bool TryGet(Type type, [NotNullWhen(true)] out SerializedType? serializedType);
	bool TryGet(string name, [NotNullWhen(true)] out SerializedType? serializedType);
    IEnumerable<SerializedType> GetAll();
}
