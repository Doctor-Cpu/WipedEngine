using Wiped.Shared.IoC;
using Wiped.Shared.Serialization;

namespace Wiped.Shared.VFS.Sources;

internal sealed class EngineContentSource : IContentSource
{
	[Dependency] private readonly IYamlManager _yaml = default!;
	[Dependency] private readonly IEnginePathsManager _paths = default!;

	private const string VFSConfigPath = "Assets/vfs.yml";

	public string Name  => "Engine";

	public VFSConfig GetConfig()
	{
		var path = Path.GetFullPath(Path.Combine(_paths.EngineRoot, VFSConfigPath));
		if (!File.Exists(path))
            throw new FileNotFoundException("Engine VFS config missing", path);

		var yamlText = File.ReadAllText(path);
		var config = _yaml.Deserialize<VFSConfig>(yamlText);
		return config;
	}

	public EngineContentSource()
	{
		IoCManager.ResolveDependencies(this);
	}
}
