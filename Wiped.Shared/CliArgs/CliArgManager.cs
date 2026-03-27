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

			var min = cliArg.MinArgs();
			var max = cliArg.MaxArgs();

			var remainingArgCount = args.Length - i;
			var takeCount = GetTakeCount(max, i, args, remainingArgCount);

			if (takeCount < min || (max != null && takeCount > max))
			{
				if (min == max)
					throw new ArgumentException($"{name} requires exactly {min} arguments, {takeCount} provided.");
				else if (max == null)
					throw new ArgumentException($"{name} requires at least {min} arguments, {takeCount} provided.");
				else
					throw new ArgumentException($"{name} requires between {min} and {max} arguments, {takeCount} provided.");
			}

			i += cliArg.Run(args[i..(i + takeCount)]);
		}
	}

	private static int GetTakeCount(int? max, int i, string[] args, int remainingArgCount)
	{
		if (max != null)
			return Math.Min(max.Value, remainingArgCount);

		var takeCount = 0;
		while (i + takeCount < args.Length)
		{
			var next = args[i + takeCount];

			if (next.StartsWith('-'))
				break;

			takeCount++;
		}

		return takeCount;
	}
}
