using Wiped.Shared.IoC;
using Wiped.Shared.Localisation;

namespace Wiped.Tools;

internal sealed class HelpTool : BaseCliTool
{
	[Dependency] private readonly IProgramManager _programs = default!;

	public override string ToolName => "help";
	public override TextLocId? ToolDesc => "tool-help-desc";

	protected override int Run(string[] args)
	{
		if (!args.Any())
		{
			foreach (var program in _programs.GetAll())
				PrintProgramHelp(program);

			return ToolExitCodes.Success;
		}

		foreach (var arg in args)
		{
			if (_programs.TryGet(arg, out var program))
				PrintProgramHelp(program);
		}

		return ToolExitCodes.Success;
	}

	private void PrintProgramHelp(BaseTool program)
	{
		PrintText($"{program.ToolName} - {program.ToolDesc}");
	}
}
