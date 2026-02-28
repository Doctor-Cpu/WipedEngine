using Wiped.Shared.CliArgs.Args;
using Wiped.Shared.IoC;

namespace Wiped.Tools;

internal sealed class ToolCliArg : BaseCliArg
{
	[Dependency] private readonly IoCDynamic<IEngineProgramManager> _program = default!;

	public override string Name() => "tool";
	public override string Desc() => "Run a specified tool";

	public override int MinArgs() => 1;
	public override int MaxArgs() => 1;

	public override int Run(string[] args)
	{
		_program.Value.SetProgram(args[0]);
		return MaxArgs();
	}
}
