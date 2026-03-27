using Wiped.Shared.Reflection;
using Wiped.Shared.Serialization.Schema;

namespace Wiped.Shared.Serialization;

[ReflectableBaseUsage]
internal interface ISerializedTypeProvider
{
    IEnumerable<SerializedType> GetSerializedTypes();
}
