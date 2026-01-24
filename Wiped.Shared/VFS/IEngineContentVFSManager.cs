using Wiped.Shared.VFS.Backends;

namespace Wiped.Shared.VFS;

internal interface IEngineContentVFSManager
{
	void Mount(IContentBackend backend);

	void LoadContent();
}
