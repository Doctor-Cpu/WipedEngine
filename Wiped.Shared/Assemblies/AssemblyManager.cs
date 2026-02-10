using System.Reflection;
using Wiped.Shared.IoC;
using Wiped.Shared.VFS;

namespace Wiped.Shared.Assemblies;

[AutoBind(typeof(IAssemblyManager))]
internal sealed class AssemblyManager : IAssemblyManager
{
	[Dependency] private readonly IHotReloadManager _hotReload = default!;

    private readonly Dictionary<Assembly, AssemblyScope> _loadedAssemblies = [];
    private readonly HashSet<string> _seenAssemblyNames = [];

	public IEnumerable<Assembly> GetAssemblies() => _loadedAssemblies.Keys.ToArray();

    public void RegisterAssembly(Assembly asm, AssemblyScope scope)
    {
		if (_seenAssemblyNames.Contains(asm.FullName!))
			return; // already registered

		_seenAssemblyNames.Add(asm.FullName!);
		_loadedAssemblies[asm] = scope;

		if (scope != AssemblyScope.Engine)
			_hotReload.Reload();
    }

	public AssemblyManager()
	{
		foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
			RegisterAssembly(asm, AssemblyScope.Engine);

		AppDomain.CurrentDomain.AssemblyLoad += (_, args) => RegisterAssembly(args.LoadedAssembly, AssemblyScope.Engine);
	}
}
