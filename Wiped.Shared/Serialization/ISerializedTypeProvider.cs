using Wiped.Shared.Reflection;
using Wiped.Shared.Serialization.Schema;

namespace Wiped.Shared.Serialization;

[Reflectable]
internal interface ISerializedTypeProvider
{
    IEnumerable<SerializedType> GetSerializedTypes();
}
