using Wiped.Shared.IoC;
using Wiped.Shared.VFS.Backends;

namespace Wiped.Shared.VFS;

internal interface IEngineContentVFSManager : IManager
{
	void Mount(IContentBackend backend);

	void Bootstrap();

	void Load(VFSConfig config);

	void UnmountAll();
}
