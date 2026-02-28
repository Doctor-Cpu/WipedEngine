namespace Wiped.Shared.Serialization;

public interface ISerializationContext
{
    object Serialize(object value);
}
