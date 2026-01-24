namespace Wiped.Shared.Serialization;

internal interface IDataDefinitionRegistryManager
{
	void Initialize();

	void Register(Type type);

	void Unregister(Type type);

    bool TryGet(string name, out Type type);

    bool TryGetName(Type type, out string name);

    IEnumerable<Type> EnumerateTypes();
}
