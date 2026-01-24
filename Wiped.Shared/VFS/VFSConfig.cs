using Wiped.Shared.Serialization;
using Wiped.Shared.VFS.Backends;

namespace Wiped.Shared.VFS;

[DataDefinition]
internal sealed class VFSConfig
{
	[DataField]
	public List<IContentBackend> Backends = [];
}
