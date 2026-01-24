using System.Reflection;
using Wiped.Shared.IoC;

namespace Wiped.Shared.Serialization;

internal sealed class DataDefinitionRegistryManager : BaseManager, IDataDefinitionRegistryManager
{
	private readonly List<Assembly> _assemblies = [];

    private readonly Dictionary<string, Type> _byName = new(StringComparer.Ordinal);
    private readonly Dictionary<Type, string> _byType = [];

	public void Initialize()
	{
		foreach (var asm in _assemblies)
		{
			foreach (var type in asm.GetTypes())
			{
				if (type.GetCustomAttribute<DataDefinitionAttribute>() is not { })
					continue;

				Register(type);
			}
		}
	}

	public void Register(Type type)
	{
		if (type.GetCustomAttribute<DataDefinitionAttribute>() is not { } attr)
        	throw new InvalidOperationException($"{type} is not a DataDefinition");

		var name = attr.Name ?? type.Name;

		if (_byName.TryGetValue(name, out var existing))
            throw new InvalidOperationException($"Duplicate DataDefinition name '{name}' for types '{existing.FullName}' and '{type.FullName}'.");

		_byName[name] = type;
		_byType[type] = name;
	}

    public void Unregister(Type type)
    {
        if (type is null)
            return;

        if (_byType.TryGetValue(type, out var name))
        {
            _byType.Remove(type);
            _byName.Remove(name);
        }
    }

    public bool TryGet(string name, out Type type) => _byName.TryGetValue(name, out type!);

    public bool TryGetName(Type type, out string name) => _byType.TryGetValue(type, out name!);

    public IEnumerable<Type> EnumerateTypes() => _byType.Keys;

	public DataDefinitionRegistryManager() : base()
	{
		_assemblies.AddRange(AppDomain.CurrentDomain.GetAssemblies());
	}
}
