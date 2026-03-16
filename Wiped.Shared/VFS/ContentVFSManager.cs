using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Wiped.Shared.IoC;

namespace Wiped.Shared.VFS;

[AutoBind(typeof(IContentVFSManager), typeof(IEngineContentVFSManager))]
internal sealed class ContentVFSManager : IContentVFSManager, IEngineContentVFSManager
{
	private List<string> _roots = default!;
	private bool _initialized;

	public void Bootstrap(params string[] roots)
	{
		if (_initialized)
			throw new InvalidOperationException("VFS already bootstrapped");

		if (!roots.Any())
			throw new InvalidOperationException($"ContentRoot not provided");

		_roots = new(roots.Length);

		foreach (var root in roots)
		{
			var fullRoot = Path.GetFullPath(root);

			if (!Directory.Exists(fullRoot))
				throw new DirectoryNotFoundException(fullRoot);

			_roots.Add(fullRoot);
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
	private string Resolve(ContentPath path, out string sourceRoot)
	{
		EnsureInitialized();
		foreach (var root in _roots)
		{
			var full = Path.Combine(root, path.ToString());
			if (!File.Exists(full) && !Directory.Exists(full))
				continue;

			sourceRoot = root;
			return full;
		}

		throw new FileNotFoundException(path.ToString());
	}

	public Stream? GetFile(ContentPath path)
	{
		var full = Resolve(path, out _);
		return File.Exists(full) ? File.OpenRead(full) : null;
	}

	public Stream GetFileOrThrow(ContentPath path)
	{
		var full = Resolve(path, out _);
		if (!File.Exists(full))
			throw new FileNotFoundException($"{path}");

		return File.OpenRead(full);
	}

	public bool TryGetFile(ContentPath path, [NotNullWhen(true)] out Stream? file)
	{
		var full = Resolve(path, out _);
		if (!File.Exists(full))
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
		var full = Resolve(folderPath, out var sourceRoot);
		if (!Directory.Exists(full))
			yield break;

		var option = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

		foreach (var file in Directory.EnumerateFiles(full, "*", option))
		{
			var relative = Path.GetRelativePath(sourceRoot, file);
			yield return new ContentPath(relative);
		}
	}
}
