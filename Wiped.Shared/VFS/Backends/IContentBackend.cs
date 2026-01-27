using System.Diagnostics.CodeAnalysis;

namespace Wiped.Shared.VFS.Backends;

internal interface IContentBackend
{
	string VFSRoot { get; set; }

	IEnumerable<ContentPath> Enumerate(bool recursive);

	bool TryOpen(ContentPath path, [NotNullWhen(true)] out Stream? stream);

	void Validate()
	{
	}
}
