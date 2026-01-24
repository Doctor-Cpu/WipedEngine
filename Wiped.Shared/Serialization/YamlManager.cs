using Wiped.Shared.IoC;
using YamlDotNet.Serialization;

namespace Wiped.Shared.Serialization;

internal sealed class YamlManager : BaseManager, IYamlManager
{
	private readonly ISerializer _serializer;
    private readonly IDeserializer _deserializer;

	public string Serialize<T>(T content)
	{
		return _serializer.Serialize(content);
	}

	public T Deserialize<T>(string content)
	{
		return _deserializer.Deserialize<T>(content);
	}

	public YamlManager() : base()
	{
		_deserializer = new DeserializerBuilder().Build();
		_serializer = new SerializerBuilder().Build();
	}
}
