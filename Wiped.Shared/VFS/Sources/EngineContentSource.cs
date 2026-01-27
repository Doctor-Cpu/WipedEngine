using Wiped.Shared.IoC;
using Wiped.Shared.Serialization;

namespace Wiped.Shared.VFS.Sources;

internal sealed class EngineContentSource : IContentSource
{
	[Dependency] private readonly IYamlManager _yaml = default!;

	private const string VFSConfigPath = "Assets/vfs.yml";

	public string Name  => "Engine";

	public VFSConfig GetConfig()
	{
		var baseDir = AppContext.BaseDirectory;
		var fullPath = Path.GetFullPath(Path.Combine(baseDir, VFSConfigPath));

		// prevent escape from baseDir
		if (!fullPath.StartsWith(baseDir, StringComparison.OrdinalIgnoreCase))
			throw new UnauthorizedAccessException($"Attempted to read file outside of engine root: {fullPath}");

		var yamlText = File.ReadAllText(fullPath);
		var config = _yaml.Deserialize<VFSConfig>(yamlText);
		return config;
	}
}
