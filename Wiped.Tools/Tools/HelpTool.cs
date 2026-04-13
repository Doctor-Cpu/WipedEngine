using Wiped.Localization.Text;
using Wiped.Shared.IoC;
using Wiped.Shared.Localization.Text;

namespace Wiped.Tools.Tools;

public sealed class HelpTool : BaseCliTool
{
	[Dependency] private readonly IoCDynamic<IEngineProgramManager> _programs = default!;
	[Dependency] private readonly IoCDynamic<ITextLocalizationManager> _textLocalization = default!;

	public override string ToolName => "help";

	private static readonly TextLocId ProgramHelpText = "tool-help-program-help";
	private static readonly TextLocId UnknownToolError = "tool-generic-error-cli-unknown-tool";

	protected override void Run(string[] args)
	{
		if (!args.Any())
		{
			foreach (var program in _programs.Value.GetAll())
				PrintProgramHelp(program);

			return;
		}

		foreach (var arg in args)
		{
			if (_programs.Value.TryGet(arg, out var program))
			{
				PrintProgramHelp(program);
			}
			else
			{
				var error = _textLocalization.Value.GetString(UnknownToolError, ("name", arg));
				PrintText(error);
			}
		}
	}

	private void PrintProgramHelp(BaseTool program)
	{
		var text = _textLocalization.Value.GetString(ProgramHelpText, ("name", program.ToolName), ("desc", program.ToolDesc));
		PrintText(text);
	}
}
