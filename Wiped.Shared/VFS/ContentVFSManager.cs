using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Wiped.Shared.IoC;

namespace Wiped.Shared.VFS;

[AutoBind(typeof(IContentVFSManager), typeof(IEngineContentVFSManager))]
internal sealed class ContentVFSManager : IContentVFSManager, IEngineContentVFSManager
{
	private bool _initialized;

	private readonly Dictionary<ContentPath, string> _files = [];
	private readonly Dictionary<ContentPath, string> _directories = [];

	private const string ApplicationName = "WipedEngine"; // TODO: replace with a project manifest with this as a field

	public void Bootstrap(params string[] roots)
	{
		if (_initialized)
			throw new InvalidOperationException("VFS already bootstrapped");

		if (!roots.Any())
			throw new InvalidOperationException($"ContentRoot not provided");

		foreach (var root in roots)
		{
			var fullRoot = Path.GetFullPath(root);

			if (!Directory.Exists(fullRoot))
				throw new DirectoryNotFoundException(fullRoot);

			foreach (var file in Directory.EnumerateFiles(fullRoot, "*", SearchOption.AllDirectories))
			{
				var relative = Path.GetRelativePath(fullRoot, file);
				var cp = new ContentPath(relative);
				_files[cp] = file;
			}

			foreach (var dir in Directory.EnumerateDirectories(fullRoot, "*", SearchOption.AllDirectories))
			{
				var relative = Path.GetRelativePath(fullRoot, dir);
				var cp = new ContentPath(relative);
				_directories[cp] = dir;
			}

			_directories[ContentPath.Root] = fullRoot;
		}

		_initialized = true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureInitialized()
    {
        if (!_initialized)
            throw new InvalidOperationException("VFS not bootstrapped.");
    }

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool TryResolveFile(ContentPath path, [NotNullWhen(true)] out string? full)
	{
		EnsureInitialized();

		return _files.TryGetValue(path, out full);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool TryResolveDirectory(ContentPath path, [NotNullWhen(true)] out string? full)
	{
		EnsureInitialized();

		return _directories.TryGetValue(path, out full);
	}

	public bool TryGetAbsolutePath(ContentPath path, [NotNullWhen(true)] out string? absolutePath) => TryResolveFile(path, out absolutePath);

	public Stream? GetFile(ContentPath path)
	{
		return TryResolveFile(path, out var full) ? File.OpenRead(full) : null;
	}

	public Stream GetFileOrThrow(ContentPath path)
	{
		if (!TryResolveFile(path, out var full))
			throw new FileNotFoundException($"{path}");

		return File.OpenRead(full);
	}

	public bool TryGetFile(ContentPath path, [NotNullWhen(true)] out Stream? file)
	{
		if (!TryResolveFile(path, out var full))
		{
			file = null;
			return false;
		}

		file = File.OpenRead(full);
		return true;
	}

	public async Task StreamFileAsync(ContentPath path, Func<Stream, Task> consumer)
	{
        await using var stream = GetFileOrThrow(path);
        await consumer(stream);
	}

    public IEnumerable<ContentPath> Enumerate(ContentPath folderPath, bool recursive = false)
	{
		EnsureInitialized();

		if (recursive)
		{
			foreach (var path in _files.Keys)
			{
				if (path.IsDescendentOf(folderPath))
					yield return path;
			}
		}
		else
		{
			foreach (var path in _files.Keys)
			{
				if (path.IsDirectChildOf(folderPath))
					yield return path;
			}
		}
	}

    public IEnumerable<ContentPath> EnumerateDirectories(ContentPath folderPath, bool recursive = false)
	{
		EnsureInitialized();

		if (recursive)
		{
			foreach (var path in _directories.Keys)
			{
				if (path.IsDescendentOf(folderPath))
					yield return path;
			}
		}
		else
		{
			foreach (var path in _directories.Keys)
			{
				if (path.IsDirectChildOf(folderPath))
					yield return path;
			}
		}
	}

    public IEnumerable<string> EnumerateAbsolute(ContentPath folderPath, bool recursive = false)
	{
		EnsureInitialized();

		if (recursive)
		{
			foreach (var (path, absolute) in _files)
			{
				if (path.IsDescendentOf(folderPath))
					yield return absolute;
			}
		}
		else
		{
			foreach (var (path, absolute) in _files)
			{
				if (path.IsDirectChildOf(folderPath))
					yield return absolute;
			}
		}
	}

    public IEnumerable<string> EnumerateDirectoriesAbsolute(ContentPath folderPath, bool recursive = false)
	{
		EnsureInitialized();

		if (recursive)
		{
			foreach (var (path, absolute) in _directories)
			{
				if (path.IsDescendentOf(folderPath))
					yield return absolute;
			}
		}
		else
		{
			foreach (var (path, absolute) in _directories)
			{
				if (path.IsDirectChildOf(folderPath))

					yield return absolute;
			}
		}
	}

	public Stream WriteDataDir(ContentPath relative) => WriteDir(relative, OSPaths.GetDataDir(ApplicationName));
	public Stream WriteConfigDir(ContentPath relative) => WriteDir(relative, OSPaths.GetConfigDir(ApplicationName));
	public Stream WriteCacheDir(ContentPath relative) => WriteDir(relative, OSPaths.GetCacheDir(ApplicationName));

	private static Stream WriteDir(ContentPath relative, string root)
	{
		var osRelative = relative.ToOsRelative();
		var full = Path.Combine(root, osRelative);

		if (Path.GetDirectoryName(full) is { } directory)
			Directory.CreateDirectory(directory);

		return new FileStream(full, FileMode.Create, FileAccess.Write, FileShare.None);
	}
}
