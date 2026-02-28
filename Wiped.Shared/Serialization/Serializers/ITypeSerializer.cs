using Wiped.Shared.Reflection;

namespace Wiped.Shared.Serialization.Serializers;

// exists purely as generics can easily be put in a list
// actual implementation that is exposed is BaseTypeSerializer
[Reflectable]
internal interface ITypeSerializer
{
	Type TargetType { get; }

	object Serialize(object value, ISerializationContext context);
	object Deserialize(object data, IDeserializationContext context);
}

