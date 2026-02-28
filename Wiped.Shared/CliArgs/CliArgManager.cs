using Wiped.Shared.CliArgs.Args;
using Wiped.Shared.IoC;
using Wiped.Shared.Lifecycle;

namespace Wiped.Shared.CliArgs;

[AutoBind(typeof(ICliArgManager))]
internal sealed class CliArgManager : ICliArgManager // no hotreloading as engine only and this is only ever done once on boot
{
	[Dependency] private readonly IoCDynamic<ILifecycleManager> _lifecycle = default!;

	public void UseArgs(string[] args)
	{
		Dictionary<string, BaseCliArg> nameToArg = new(StringComparer.OrdinalIgnoreCase);
		foreach (var arg in _lifecycle.Value.GetAll<BaseCliArg>())
			nameToArg[arg.Name()] = arg;

		var i = 0;

		while (i < args.Length)
		{
    		var name = args[i].TrimStart('-');
			i++;

			if (!nameToArg.TryGetValue(name, out var cliArg))
				throw new ArgumentException($"Unknown argument: {name}");

			var remainingArgCount = args.Length - i;
			if (remainingArgCount < cliArg.MinArgs() || remainingArgCount > cliArg.MaxArgs())
				throw new ArgumentException($"{name} requires between {cliArg.MinArgs()} and {cliArg.MaxArgs()} arguments, {remainingArgCount} provided.");

        	var takeCount = Math.Min(cliArg.MaxArgs(), remainingArgCount);
			i += cliArg.Run(args[i..(i + takeCount)]);
		}
	}
}
