using Wiped.Shared.IoC;
using YamlDotNet.Serialization;

namespace Wiped.Shared.Serialization;

[AutoBind(typeof(IYamlManager))]
internal sealed class YamlManager : IManager, IYamlManager
{
	private ISerializer _serializer = default!;
    private IDeserializer _deserializer = default!;

	public void Initialize()
	{
		_deserializer = new DeserializerBuilder().Build();
		_serializer = new SerializerBuilder().Build();
	}

	public string Serialize<T>(T content)
	{
		return _serializer.Serialize(content);
	}

	public T Deserialize<T>(string content)
	{
		return _deserializer.Deserialize<T>(content);
	}
}
