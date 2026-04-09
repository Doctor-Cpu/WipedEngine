using System.Diagnostics.CodeAnalysis;
using Wiped.Shared.IoC;

namespace Wiped.Shared.VFS;

internal interface IEngineContentVFSManager : IManager
{
	void Bootstrap(params string[] roots);

	bool TryGetAbsolutePath(ContentPath path, [NotNullWhen(true)] out string? absolutePath);
	bool TryGetAbsolutePath(ContentPath path, ContentLocation location, [NotNullWhen(true)] out string? absolutePath);

    IEnumerable<string> EnumerateAbsolute(ContentPath folderPath, bool recursive = false);
    IEnumerable<string> EnumerateAbsolute(ContentPath folderPath, ContentLocation location, bool recursive = false);

    IEnumerable<string> EnumerateDirectoriesAbsolute(ContentPath folderPath, bool recursive = false);
    IEnumerable<string> EnumerateDirectoriesAbsolute(ContentPath folderPath, ContentLocation location, bool recursive = false);

	Stream Write(ContentPath relative, ContentLocation location);

	bool TryGetFile(ContentPath path, ContentLocation location, [NotNullWhen(true)] out Stream? file);

	Stream? GetFile(ContentPath path, ContentLocation location);

	Stream GetFileOrThrow(ContentPath path, ContentLocation location);

	Task StreamFileAsync(ContentPath path, ContentLocation location, Func<Stream, Task> consumer);

    IEnumerable<ContentPath> Enumerate(ContentPath folderPath, ContentLocation location, bool recursive = false);
    IEnumerable<ContentPath> EnumerateDirectories(ContentPath folderPath, ContentLocation location, bool recursive = false);
}
