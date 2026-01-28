using Wiped.Shared.IoC;

namespace Wiped.Shared.VFS;

[AutoBind(typeof(IEnginePathsManager))]
internal sealed class EnginePathsManager : IManager, IEnginePathsManager
{
    public string ProcessRoot { get; }
    public string EngineRoot { get; }
    public string ProjectRoot { get; }
    public string UserDataRoot { get; }

	public const string EngineRootMarker = ".engine-root";
	public const string EngineSubmoduleName = "WipedEngine";

    public EnginePathsManager()
    {
        ProcessRoot = AppContext.BaseDirectory;
        ProjectRoot = Directory.GetCurrentDirectory();
        EngineRoot  = FindEngineRoot(ProcessRoot, ProjectRoot);
        UserDataRoot = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Wiped");
    }

	private static string FindEngineRoot(string processRoot, string projectRoot)
	{
		var dir = new DirectoryInfo(processRoot);
		while (dir != null)
		{
			if (IsEngineRoot(dir.FullName))
				return dir.FullName;

			dir = dir.Parent;
		}

		var engineSubmodule = Path.Combine(projectRoot, EngineSubmoduleName);
		if (Directory.Exists(engineSubmodule) && IsEngineRoot(engineSubmodule))
			return engineSubmodule;

		throw new InvalidOperationException(
			$"Unable to locate engine root.\n" +
			$"ProcessRoot: {processRoot}\n" +
			$"ProjectRoot: {projectRoot}\n" +
			$"Expected marker '.engine-root' or known engine files."
		);
	}

	private static bool IsEngineRoot(string path)
	{
		// explicit marker
		if (File.Exists(Path.Combine(path, EngineRootMarker)))
			return true;

		return false;
	}
}
