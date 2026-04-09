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
	private bool TryResolveFile(ContentPath path, ContentLocation location, [NotNullWhen(true)] out string? full)
	{
		switch (location)
		{
			case ContentLocation.Content:
				EnsureInitialized();
				return _files.TryGetValue(path, out full);

			case ContentLocation.Data:
			case ContentLocation.Config:
			case ContentLocation.Cache:
				var root = ResolveRoot(location);
				full = Path.Combine(root, path.ToOsRelative());
				return File.Exists(full);

			default:
				throw new NotImplementedException();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool TryResolveDirectory(ContentPath path, ContentLocation location, [NotNullWhen(true)] out string? full)
	{
		switch (location)
		{
			case ContentLocation.Content:
				EnsureInitialized();
				return _directories.TryGetValue(path, out full);

			case ContentLocation.Data:
			case ContentLocation.Config:
			case ContentLocation.Cache:
				var root = ResolveRoot(location);
				full = Path.Combine(root, path.ToOsRelative());
				return Directory.Exists(full);

			default:
				throw new NotImplementedException();
		}
	}

	public bool TryGetAbsolutePath(ContentPath path, [NotNullWhen(true)] out string? absolutePath) => TryGetAbsolutePath(path, ContentLocation.Content, out absolutePath);
	public bool TryGetAbsolutePath(ContentPath path, ContentLocation location, [NotNullWhen(true)] out string? absolutePath) => TryResolveFile(path, location, out absolutePath);

	public Stream? GetFile(ContentPath path) => GetFile(path, ContentLocation.Content);
	public Stream? GetFile(ContentPath path, ContentLocation location)
	{
		return TryResolveFile(path, location, out var full) ? File.OpenRead(full) : null;
	}

	public Stream GetFileOrThrow(ContentPath path) => GetFileOrThrow(path, ContentLocation.Content);
	public Stream GetFileOrThrow(ContentPath path, ContentLocation location)
	{
		if (!TryResolveFile(path, location, out var full))
			throw new FileNotFoundException($"{path}");

		return File.OpenRead(full);
	}

	public bool TryGetFile(ContentPath path, [NotNullWhen(true)] out Stream? file) => TryGetFile(path, ContentLocation.Content, out file);
	public bool TryGetFile(ContentPath path, ContentLocation location, [NotNullWhen(true)] out Stream? file)
	{
		if (!TryResolveFile(path, location, out var full))
		{
			file = null;
			return false;
		}

		file = File.OpenRead(full);
		return true;
	}

	public async Task StreamFileAsync(ContentPath path, Func<Stream, Task> consumer) => await StreamFileAsync(path, ContentLocation.Content, consumer);
	public async Task StreamFileAsync(ContentPath path, ContentLocation location, Func<Stream, Task> consumer)
	{
        await using var stream = GetFileOrThrow(path, location);
        await consumer(stream);
	}

    public IEnumerable<ContentPath> Enumerate(ContentPath folderPath, bool recursive = false) => Enumerate(folderPath, ContentLocation.Content, recursive);
    public IEnumerable<ContentPath> Enumerate(ContentPath folderPath, ContentLocation location, bool recursive = false)
	{
		switch (location)
		{
			case ContentLocation.Content:
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

				yield break;

			case ContentLocation.Data:
			case ContentLocation.Config:
			case ContentLocation.Cache:
				var root = ResolveRoot(location);
				var basePath = Path.Combine(root, folderPath.ToOsRelative());

				if (!Directory.Exists(basePath))
					yield break;

				var option = recursive
					? SearchOption.AllDirectories
					: SearchOption.TopDirectoryOnly;

				foreach (var path in Directory.EnumerateFiles(basePath, "*", option))
				{
					var relative = Path.GetRelativePath(root, path);
					yield return new ContentPath(relative);
				}

				yield break;

			default:
				throw new NotImplementedException();
		}
	}

    public IEnumerable<ContentPath> EnumerateDirectories(ContentPath folderPath, bool recursive = false) => EnumerateDirectories(folderPath, ContentLocation.Content, recursive);
    public IEnumerable<ContentPath> EnumerateDirectories(ContentPath folderPath, ContentLocation location, bool recursive = false)
	{
		switch (location)
		{
			case ContentLocation.Content:
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

				yield break;

			case ContentLocation.Data:
			case ContentLocation.Config:
			case ContentLocation.Cache:
				var root = ResolveRoot(location);
				var basePath = Path.Combine(root, folderPath.ToOsRelative());

				if (!Directory.Exists(basePath))
					yield break;

				var option = recursive
					? SearchOption.AllDirectories
					: SearchOption.TopDirectoryOnly;

				foreach (var dir in Directory.EnumerateDirectories(basePath, "*", option))
				{
					var relative = Path.GetRelativePath(root, dir);
					yield return new ContentPath(relative);
				}

				yield break;

			default:
				throw new NotImplementedException();
		}
	}

    public IEnumerable<string> EnumerateAbsolute(ContentPath folderPath, bool recursive = false) => EnumerateAbsolute(folderPath, ContentLocation.Content, recursive);
    public IEnumerable<string> EnumerateAbsolute(ContentPath folderPath, ContentLocation location, bool recursive = false)
	{
		switch (location)
		{
			case ContentLocation.Content:
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

				yield break;

			case ContentLocation.Data:
			case ContentLocation.Config:
			case ContentLocation.Cache:
				var root = ResolveRoot(location);
				var basePath = Path.Combine(root, folderPath.ToOsRelative());

				if (!Directory.Exists(basePath))
					yield break;

				var option = recursive
					? SearchOption.AllDirectories
					: SearchOption.TopDirectoryOnly;

				foreach (var path in Directory.EnumerateFiles(basePath, "*", option))
					yield return path;

				yield break;

			default:
				throw new NotImplementedException();
		}
	}

    public IEnumerable<string> EnumerateDirectoriesAbsolute(ContentPath folderPath, bool recursive = false) => EnumerateDirectoriesAbsolute(folderPath, ContentLocation.Content, recursive);
    public IEnumerable<string> EnumerateDirectoriesAbsolute(ContentPath folderPath, ContentLocation location, bool recursive = false)
	{
		switch (location)
		{
			case ContentLocation.Content:
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

				yield break;

			case ContentLocation.Data:
			case ContentLocation.Config:
			case ContentLocation.Cache:
				var root = ResolveRoot(location);
				var basePath = Path.Combine(root, folderPath.ToOsRelative());

				if (!Directory.Exists(basePath))
					yield break;

				var option = recursive
					? SearchOption.AllDirectories
					: SearchOption.TopDirectoryOnly;

				foreach (var dir in Directory.EnumerateDirectories(basePath, "*", option))
					yield return dir;

				yield break;

			default:
				throw new NotImplementedException();
		}
	}

	public Stream Write(ContentPath relative, ContentLocation location)
	{
		var root = ResolveRoot(location);

		var osRelative = relative.ToOsRelative();
		var full = Path.Combine(root, osRelative);

		if (Path.GetDirectoryName(full) is { } directory)
			Directory.CreateDirectory(directory);

		return new FileStream(full, FileMode.Create, FileAccess.Write, FileShare.None);
	}

	private string ResolveRoot(ContentLocation location)
	{
		return location switch
		{
			ContentLocation.Content => throw new InvalidOperationException(),
			ContentLocation.Data => OSPaths.GetDataDir(ApplicationName),
			ContentLocation.Config => OSPaths.GetConfigDir(ApplicationName),
			ContentLocation.Cache => OSPaths.GetCacheDir(ApplicationName),
			_ => throw new NotImplementedException()
		};
	}
}
