using System.Diagnostics.CodeAnalysis;
using Wiped.Shared.IoC;
using Wiped.Shared.Lifecycle;
using Wiped.Shared.Serialization.Schema;
using Wiped.Shared.VFS;

namespace Wiped.Shared.Serialization;

[AutoBind(typeof(ISerializedTypeRegistryManager))]
internal sealed class SerialiedTypeRegistryManager : ISerializedTypeRegistryManager, IHotReloadable
{
	[Dependency] private readonly IoCDynamic<ILifecycleManager> _lifecycle = default!;

	public Type[] After => [typeof(ILifecycleManager)];

	private readonly Dictionary<Type, SerializedType> _byType = [];
	private readonly Dictionary<string, SerializedType> _byName = new(StringComparer.Ordinal);

	public void Initialize()
	{
		foreach (var provider in _lifecycle.Value.GetAll<ISerializedTypeProvider>())
		{
			foreach (var serializedType in provider.GetSerializedTypes())
				Register(serializedType);
		}
	}

	public void Shutdown()
	{
		_byName.Clear();
		_byType.Clear();
	}

	public void Register(SerializedType type)
	{
		_byType[type.ClrType] = type;
		_byName[type.Name] = type;
	}

    public bool TryGet(Type type, [NotNullWhen(true)] out SerializedType? serializedType) => _byType.TryGetValue(type, out serializedType);
    public bool TryGet(string name, [NotNullWhen(true)] out SerializedType? serializedType) => _byName.TryGetValue(name, out serializedType);
	public IEnumerable<SerializedType> GetAll() => _byType.Values;
}
