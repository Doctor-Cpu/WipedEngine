using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Wiped.Shared.IoC;

namespace Wiped.Shared.VFS;

[AutoBind(typeof(IContentVFSManager), typeof(IEngineContentVFSManager))]
internal sealed class ContentVFSManager : IContentVFSManager, IEngineContentVFSManager
{
	private string _root = default!;
	private bool _initialized;

	public void Bootstrap(string root)
	{
		if (_initialized)
			throw new InvalidOperationException("VFS already bootstrapped");

		if (string.IsNullOrWhiteSpace(root))
			throw new InvalidOperationException($"ContentRoot not provided");

		_root = Path.GetFullPath(root);

		if (!Directory.Exists(_root))
			throw new DirectoryNotFoundException(_root);

		_initialized = true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureInitialized()
    {
        if (!_initialized)
            throw new InvalidOperationException("VFS not bootstrapped.");
    }

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private string Resolve(ContentPath path)
	{
		EnsureInitialized();
		return Path.Combine(_root, path.ToString());
	}

	public Stream? GetFile(ContentPath path)
	{
		var full = Resolve(path);
		return File.Exists(full) ? File.OpenRead(full) : null;
	}

	public Stream GetFileOrThrow(ContentPath path)
	{
		var full = Resolve(path);
		if (!File.Exists(full))
			throw new FileNotFoundException($"{path}");

		return File.OpenRead(full);
	}

	public bool TryGetFile(ContentPath path, [NotNullWhen(true)] out Stream? file)
	{
		var full = Resolve(path);
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
		var full = Resolve(folderPath);
		if (!Directory.Exists(full))
			yield break;

		var option = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

		foreach (var file in Directory.EnumerateFiles(full, "*", option))
		{
			var relative = Path.GetRelativePath(_root, file);
			yield return new ContentPath(relative);
		}
	}
}
