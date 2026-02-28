using Wiped.Shared.CliArgs;
using Wiped.Shared.IoC;
using Wiped.Shared.VFS;

namespace Wiped.Shared;

public static class EntryPoint
{
	public static void Start(IGeneratedModule rootModule, string[] args)
	{
		ModuleRunner.RunIoC(rootModule);
		IoCManager.AllowResolving();
		IoCManager.CreateInstances();

		var hotReload = IoCManager.Resolve<IHotReloadManager>().Value;
		hotReload.Initialize();

		var cli = IoCManager.Resolve<ICliArgManager>().Value;
		cli.UseArgs(args);
	}
}
