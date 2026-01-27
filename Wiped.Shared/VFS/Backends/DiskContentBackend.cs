using System.Diagnostics.CodeAnalysis;
using Wiped.Shared.Serialization;

namespace Wiped.Shared.VFS.Backends;

[DataDefinition]
internal sealed class DiskContentBackend : IContentBackend
{
	[DataField("path", required: true)]
	public string AbsolutePath = default!;

	[DataField(required: true)]
	public string VFSRoot { get; set; } = default!;

	public IEnumerable<ContentPath> Enumerate(bool recursive)
	{
		var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

		foreach (var file in Directory.EnumerateFiles(AbsolutePath, "*", searchOption))
		{
			var relative = Path.GetRelativePath(AbsolutePath, file);
			yield return new ContentPath(relative);
		}
	}

	public bool TryOpen(ContentPath path, [NotNullWhen(true)] out Stream? stream)
	{
		var fullPath = Path.Combine(AbsolutePath, path.ToString());

		if (!File.Exists(fullPath))
		{
			stream = null;
			return false;
		}

		stream = File.OpenRead(fullPath);
		return true;
	}
}
