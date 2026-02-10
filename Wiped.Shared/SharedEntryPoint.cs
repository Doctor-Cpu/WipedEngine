using System.Reflection;
using Wiped.Shared.IoC;
using Wiped.Shared.VFS;

namespace Wiped.Shared;

internal static class SharedEntryPoint
{
	internal static void Start()
	{
        LoadAllEngineAssemblies();

		IoCManager.AutoBindEngine();
		IoCManager.EngineTransitionTo(IoCLifecycle.Resolving);
		IoCManager.CreateInstancesEngine();

		var hotReload = IoCManager.Resolve<IHotReloadManager>();
		hotReload.Initialize();
	}

	internal static void Initialize()
	{
		var vfs = IoCManager.Resolve<IEngineContentVFSManager>();
		vfs.Bootstrap();

        IoCManager.ContentTransitionTo(IoCLifecycle.Resolving);

		IoCManager.EngineTransitionTo(IoCLifecycle.Frozen);
		IoCManager.ContentTransitionTo(IoCLifecycle.Frozen);
	}

	private static void LoadAllEngineAssemblies()
	{
		if (Assembly.GetEntryAssembly() is not { } entry)
			return;

		HashSet<string> loaded = [];
		LoadAssembly(entry, loaded);
	}

	private static void LoadAssembly(Assembly asm, HashSet<string> loaded)
	{
		if (!loaded.Add(asm.FullName!))
			return;

		foreach (var reference in asm.GetReferencedAssemblies())
		{
			try
			{
				var refAsm = Assembly.Load(reference);
				LoadAssembly(refAsm, loaded);
			}
			catch
			{
			}
		}
	}
}
