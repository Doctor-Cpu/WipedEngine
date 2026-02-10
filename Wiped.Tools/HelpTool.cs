using Wiped.Localisation;
using Wiped.Shared.IoC;
using Wiped.Shared.Localisation;

namespace Wiped.Tools;

internal sealed class HelpTool : BaseCliTool
{
	[Dependency] private readonly IProgramManager _programs = default!;
	[Dependency] private readonly ITextLocalisationManager _textLocalisation = default!;

	public override string ToolName => "help";
	public override TextLocId? ToolDesc => "tool-help-desc";

	private static readonly TextLocId ProgramHelpText = "tool-help-program-help";

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
		var text = _textLocalisation.GetString(ProgramHelpText, ("name", program.ToolName), ("desc", program.ToolDesc));
		PrintText($"{program.ToolName} - {program.ToolDesc}");
	}
}
