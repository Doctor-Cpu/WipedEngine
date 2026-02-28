using Wiped.Shared.IoC;
using Wiped.Shared.VFS;

namespace Wiped.Shared.Serialization.Disk;

public interface IDiskSerializationManager : IManager
{
	void Save<T>(ContentPath path, T obj);
	T Load<T>(ContentPath path);
	T Load<T>(Stream stream);
}
