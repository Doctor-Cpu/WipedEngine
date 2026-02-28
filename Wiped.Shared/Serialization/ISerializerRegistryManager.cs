using System.Diagnostics.CodeAnalysis;
using Wiped.Shared.IoC;
using Wiped.Shared.Serialization.Serializers;

namespace Wiped.Shared.Serialization;

internal interface ISerializerRegistryManager : IManager
{
	bool TryGetSerializer<T>([NotNullWhen(true)] out BaseTypeSerializer<T>? serializer);
	bool TryGetSerializer(Type type, [NotNullWhen(true)] out ITypeSerializer? serializer);
}
