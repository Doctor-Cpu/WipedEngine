namespace Wiped.Tools;

public sealed class ExitTool : BaseCliTool
{
	public override string ToolName => "exit";

	protected override void Run(string[] args)
	{
		Environment.Exit(0);
	}
}
