namespace Wiped.Shared.VFS.Sources;

internal interface IContentSource
{
	string Name { get; }

	VFSConfig GetConfig();
}
