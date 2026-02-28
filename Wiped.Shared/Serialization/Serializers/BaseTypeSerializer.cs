namespace Wiped.Shared.Serialization.Serializers;

public abstract class BaseTypeSerializer<T> : ITypeSerializer
{
	Type ITypeSerializer.TargetType => typeof(T);

	object ITypeSerializer.Serialize(object value, ISerializationContext context) => Serialize((T)value, context);
	object ITypeSerializer.Deserialize(object data, IDeserializationContext context) => Deserialize(data, context)!;

	public abstract object Serialize(T value, ISerializationContext context);

	public abstract T Deserialize(object data, IDeserializationContext context);
}
