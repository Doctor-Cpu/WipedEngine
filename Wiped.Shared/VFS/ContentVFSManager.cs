using System.Diagnostics.CodeAnalysis;
using Wiped.Shared.IoC;
using Wiped.Shared.VFS.Backends;
using Wiped.Shared.VFS.Sources;

namespace Wiped.Shared.VFS;

[AutoBind(typeof(IContentVFSManager), typeof(IEngineContentVFSManager))]
internal sealed class ContentVFSManager : IManager, IContentVFSManager, IEngineContentVFSManager
{
    private readonly Dictionary<ContentPath, IContentBackend> _lookup = [];

	public void Bootstrap()
	{
		var bootstrapSource = new EngineContentSource();
		var bootstrapConfig = bootstrapSource.GetConfig();
		Load(bootstrapConfig);
	}
	
	public void Load(VFSConfig config)
	{
		UnmountAll();

		foreach (var backend in config.Backends)
			Mount(backend);
	}

	public void Mount(IContentBackend backend)
	{
		backend.Validate();

		foreach (var path in backend.Enumerate(true))
		{
			if (_lookup.ContainsKey(path))
                throw new InvalidOperationException($"Duplicate content path {path}");

			_lookup[path] = backend;
		}
	}

	public void UnmountAll()
	{
		_lookup.Clear();
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
