using Wiped.Localization.Text;
using Wiped.Shared.IoC;
using Wiped.Shared.Localization.Text;

namespace Wiped.Tools;

public sealed class UsageTool : BaseCliTool
{
	[Dependency] private readonly IoCDynamic<IEngineProgramManager> _programs = default!;
	[Dependency] private readonly IoCDynamic<ITextLocalizationManager> _textLocalization = default!;

	public override string ToolName => "usage";

	private static readonly TextLocId UsageEntry = "tool-usage-program-usage";
	private static readonly TextLocId UnknownToolError = "tool-generic-error-cli-unknown-tool";
	private static readonly TextLocId MinArgError = "tool-generic-error-cli-little-args";

	protected override void Run(string[] args)
	{
		if (args.Length < 1)
		{
			var error = _textLocalization.Value.GetString(MinArgError, ("expected", 1), ("got", args.Length));
			PrintText(error);
			return;
		}

		foreach (var arg in args)
		{
			if (_programs.Value.TryGet(arg, out var tool))
			{
				PrintProgramUsage(tool);
			}
			else
			{
				var error = _textLocalization.Value.GetString(UnknownToolError, ("name", arg));
				PrintText(error);
			}
		}
	}

	private void PrintProgramUsage(BaseTool program)
	{
		var text = _textLocalization.Value.GetString(UsageEntry, ("name", program.ToolName), ("usage", program.ToolUsage));
		PrintText(text);
	}
}
