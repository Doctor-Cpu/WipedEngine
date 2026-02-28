using Wiped.Shared.IoC;

namespace Wiped.Shared.Serialization.Disk;

public sealed class DiskDeserializationContext : IDeserializationContext
{
	[Dependency] private readonly IoCDynamic<ISerializerRegistryManager> _serializerRegistry = default!;

	private readonly BinaryReader _reader = default!;

    public object Deserialize<T>() => (T)Deserialize(typeof(T));

    public object Deserialize(Type type)
	{
		/*
		if (_serializerRegistry.TryGetSerializer(type, out var serializer))
			return serializer.Deserialize(this);
		*/

		throw new NotImplementedException();
	}

	internal DiskDeserializationContext(Stream stream)
	{
		IoCManager.ResolveDependencies(this);
		_reader = new(stream);
	}
}
