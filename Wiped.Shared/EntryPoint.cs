using Wiped.Shared.CliArgs;
using Wiped.Shared.IoC;
using Wiped.Shared.Reflection;
using Wiped.Shared.VFS;

namespace Wiped.Shared;

public static class EntryPoint
{
	public static void Start(IGeneratedModule rootModule, string[] args)
	{
		ModuleRunner.RegisterIoC(rootModule);
		IoCManager.AllowResolving();
		IoCManager.CreateInstances();

		var reflection = IoCManager.Resolve<IEngineReflectionManager>().Value;
		reflection.ConsumeRegistry(rootModule);

		var hotReload = IoCManager.Resolve<IHotReloadManager>().Value;
		hotReload.Initialize();

		var cli = IoCManager.Resolve<ICliArgManager>().Value;
		cli.UseArgs(args);
	}
}
