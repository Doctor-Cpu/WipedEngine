using Wiped.Shared.IoC;

namespace Wiped.Shared.VFS;

internal interface IEngineContentVFSManager : IManager
{
	void Bootstrap(params string[] roots);
}
