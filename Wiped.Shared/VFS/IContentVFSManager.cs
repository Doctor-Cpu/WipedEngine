using System.Diagnostics.CodeAnalysis;

namespace Wiped.Shared.VFS;

public interface IContentVFSManager
{
	bool TryGetFile(ContentPath path, [NotNullWhen(true)] out Stream? file);

	Task StreamFileAsync(ContentPath path, Func<Stream, Task> consumer);

    IEnumerable<ContentPath> Enumerate(ContentPath folderPath, bool recursive = false);
}
