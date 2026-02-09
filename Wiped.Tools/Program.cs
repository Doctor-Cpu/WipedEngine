using Wiped.Shared;
using Wiped.Shared.IoC;

namespace Wiped.Tools;

internal class Program
{
	static int Main(string[] args)
	{
		SharedEntryPoint.Start();
		SharedEntryPoint.Initialize();

		var programLoader = IoCManager.Resolve<IProgramManager>();

		if (args.Length < 1)
			return programLoader.Load<HelpTool>([]);
		else
			return programLoader.Load(args[0], args[1 ..]);
	}
}
