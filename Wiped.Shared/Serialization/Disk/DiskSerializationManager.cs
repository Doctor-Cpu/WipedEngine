using Wiped.Shared.IoC;
using Wiped.Shared.VFS;

namespace Wiped.Shared.Serialization.Disk;

[AutoBind(typeof(IDiskSerializationManager))]
internal sealed class DiskSerializationManager : IDiskSerializationManager
{
	[Dependency] private readonly IoCDynamic<IContentVFSManager> _vfs = default!;

	public void Save<T>(ContentPath path, T obj)
	{
	}

	public T Load<T>(ContentPath path)
	{
		var stream = _vfs.Value.GetFileOrThrow(path);
		return Load<T>(stream);
	}

	public T Load<T>(Stream stream)
	{
		var context = new DiskDeserializationContext(stream);
		return (T)context.Deserialize<T>();
	}
}
