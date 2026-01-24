namespace Wiped.Shared.Serialization;

public interface IYamlManager
{
	string Serialize<T>(T content);
	T Deserialize<T>(string content);
}
