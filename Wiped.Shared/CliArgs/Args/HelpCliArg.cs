using Wiped.Shared.IoC;
using Wiped.Shared.Lifecycle;

namespace Wiped.Shared.CliArgs.Args;

internal sealed class HelpCliArg : BaseCliArg
{
	[Dependency] private readonly IoCDynamic<ILifecycleManager> _lifecycle = default!;

	public override string Name() => "help";
    public override string Desc() => "Show all available arguments available.";

	public override int Run(string[] args)
	{
		foreach (var arg in _lifecycle.Value.GetAll<BaseCliArg>())
			Console.WriteLine(arg.Usage());

		EntryPoint.Stop();
		return 0;
	}
}
