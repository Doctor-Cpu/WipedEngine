using System.Diagnostics.CodeAnalysis;
using Wiped.Shared.IoC;

namespace Wiped.Shared.VFS;

public interface IContentVFSManager : IManager
{
	bool TryGetFile(ContentPath path, [NotNullWhen(true)] out Stream? file);

	Stream? GetFile(ContentPath path);

	Stream GetFileOrThrow(ContentPath path);

	Task StreamFileAsync(ContentPath path, Func<Stream, Task> consumer);

    IEnumerable<ContentPath> Enumerate(ContentPath folderPath, bool recursive = false);
}
