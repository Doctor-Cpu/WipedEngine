using Wiped.Shared.IoC;

namespace Wiped.Shared.Serialization.Disk;

public sealed class DiskSerializationContext : ISerializationContext
{
	[Dependency] private readonly IoCDynamic<ISerializerRegistryManager> _serializerRegistry = default!;

	private readonly BinaryWriter _writer = default!;

	public object Serialize(object value)
	{
		if (value == null)
			throw new ArgumentNullException(nameof(value));

		var type = value.GetType();

		// custom one exists
		// go use it silly
		if (_serializerRegistry.Value.TryGetSerializer(type, out var serializer))
			return serializer.Serialize(value, this);

		throw new NotImplementedException();
	}

	internal DiskSerializationContext(Stream stream)
	{
		IoCManager.ResolveDependencies(this);
		_writer = new(stream);
	}
}
