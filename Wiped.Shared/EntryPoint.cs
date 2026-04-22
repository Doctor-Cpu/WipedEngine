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

		var cli = IoCManager.Resolve<ICliArgManager>().Value;
		cli.UseArgs(args);

		var hotReload = IoCManager.Resolve<IHotReloadManager>().Value;
		hotReload.Initialize();

		Console.CancelKeyPress += (_, _) => Stop();
	}

	public static void Stop() => Stop(Environment.ExitCode);

	public static void Stop(int exitCode)
	{
		var hotReload = IoCManager.Resolve<IHotReloadManager>().Value;
		hotReload.Shutdown();
		Environment.Exit(exitCode);
	}
}
