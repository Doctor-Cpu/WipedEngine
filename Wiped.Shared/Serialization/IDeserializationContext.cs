namespace Wiped.Shared.Serialization;

public interface IDeserializationContext
{
    object Deserialize<T>();
    object Deserialize(Type type);
}
