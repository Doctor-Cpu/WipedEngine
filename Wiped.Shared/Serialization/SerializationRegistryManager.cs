using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Wiped.Shared.IoC;
using Wiped.Shared.Lifecycle;
using Wiped.Shared.Serialization.Serializers;
using Wiped.Shared.VFS;
using DependencyAttribute = Wiped.Shared.IoC.DependencyAttribute;

namespace Wiped.Shared.Serialization;

[AutoBind(typeof(ISerializerRegistryManager))]
internal sealed class SerializationRegistryManager : ISerializerRegistryManager, IHotReloadable
{
	[Dependency] private readonly IoCDynamic<ILifecycleManager> _lifecycle = default!;

	public static Type[] After => [typeof(ILifecycleManager)];

	private readonly Dictionary<Type, ITypeSerializer> _serializers = [];

	public void Initialize()
	{
		foreach (var serializer in _lifecycle.Value.GetAll<ITypeSerializer>())
			RegisterSerializer(serializer);
	}
	
	public void Shutdown()
	{
		_serializers.Clear();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void RegisterSerializer(ITypeSerializer serializer)
	{
		_serializers[serializer.TargetType] = serializer;
	}

	public bool TryGetSerializer(Type type, [NotNullWhen(true)] out ITypeSerializer? serializer)
	{
		return _serializers.TryGetValue(type, out serializer);
	}

	public bool TryGetSerializer<T>([NotNullWhen(true)] out BaseTypeSerializer<T>? serializer)
	{
		if (_serializers.TryGetValue(typeof(T), out var s))
		{
			serializer = (BaseTypeSerializer<T>)s;
			return true;
		}

		serializer = null;
		return false;
	}
}
