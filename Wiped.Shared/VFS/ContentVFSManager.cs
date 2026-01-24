using System.Diagnostics.CodeAnalysis;
using Wiped.Shared.IoC;
using Wiped.Shared.Serialization;
using Wiped.Shared.VFS.Backends;

namespace Wiped.Shared.VFS;

internal sealed class ContentVFSManager : BaseManager, IContentVFSManager, IEngineContentVFSManager
{
	[Dependency] private readonly IYamlManager _yaml = default!;

	private const string VFSConfigPath = "Assets/vfs.yml";

    private readonly Dictionary<ContentPath, IContentBackend> _lookup = [];

	public void LoadContent()
	{
		var baseDir = AppContext.BaseDirectory;
		var fullPath = Path.GetFullPath(Path.Combine(baseDir, VFSConfigPath));

		// prevent escape from baseDir
		if (!fullPath.StartsWith(baseDir, StringComparison.OrdinalIgnoreCase))
			throw new UnauthorizedAccessException($"Attempted to read file outside of engine root: {fullPath}");

		var yamlText = File.ReadAllText(fullPath);
		var config = _yaml.Deserialize<VFSConfig>(yamlText);

		foreach (var backend in config.Backends)
			Mount(backend);
	}

	public void Mount(IContentBackend backend)
	{
		foreach (var path in backend.Enumerate(true))
		{
			if (_lookup.ContainsKey(path))
                throw new InvalidOperationException($"Duplicate content path {path}");

			_lookup[path] = backend;
		}
	}

	public bool TryGetFile(ContentPath path, [NotNullWhen(true)] out Stream? file)
	{
		if (!_lookup.TryGetValue(path, out var backend))
		{
			file = null;
			return false;
		}

		return backend.TryOpen(path, out file);
	}

	public async Task StreamFileAsync(ContentPath path, Func<Stream, Task> consumer)
	{
		if (!TryGetFile(path, out var stream))
			throw new FileNotFoundException(path.ToString());

		await using (stream)
			await consumer(stream);
	}

    public IEnumerable<ContentPath> Enumerate(ContentPath folderPath, bool recursive = false)
	{
		foreach (var path in _lookup.Keys)
		{
			if (recursive)
			{
				if (!path.IsDescendentOf(folderPath))
					continue;
			}
			else
			{
				if (!path.IsDirectChildOf(folderPath))
					continue;
			}

			yield return path;
		}
	}
}
