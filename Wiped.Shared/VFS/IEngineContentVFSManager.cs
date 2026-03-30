using System.Diagnostics.CodeAnalysis;
using Wiped.Shared.IoC;

namespace Wiped.Shared.VFS;

internal interface IEngineContentVFSManager : IManager
{
	void Bootstrap(params string[] roots);
	bool TryGetAbsolutePath(ContentPath path, [NotNullWhen(true)] out string? absolutePath);
    IEnumerable<string> EnumerateAbsolute(ContentPath folderPath, bool recursive = false);
    IEnumerable<string> EnumerateDirectoriesAbsolute(ContentPath folderPath, bool recursive = false);

	Stream WriteDataDir(ContentPath relative);
	Stream WriteConfigDir(ContentPath relative);
	Stream WriteCacheDir(ContentPath relative);
}
