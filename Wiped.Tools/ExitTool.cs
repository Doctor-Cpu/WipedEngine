namespace Wiped.Tools;

public sealed class ExitTool : BaseCliTool
{
	public override string ToolName => "exit";

	protected override void Run(string[] args)
	{
		if (args.Length >= 1 && int.TryParse(args[0], out var exitCode))
			Environment.Exit(exitCode);
		else
			Environment.Exit(0);
	}
}
