using Wiped.Shared.IoC;
using Wiped.Shared.VFS;

namespace Wiped.Shared;

internal static class SharedEntryPoint
{
	internal static void Start()
	{
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
}
