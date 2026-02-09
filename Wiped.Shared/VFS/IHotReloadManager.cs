using Wiped.Shared.IoC;

namespace Wiped.Shared.VFS;

internal interface IHotReloadManager : IManager
{
	void Initialize();

	void Shutdown();
}
