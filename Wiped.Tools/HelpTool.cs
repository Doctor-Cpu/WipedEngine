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
		foreach (var program in _programs.GetAll())
			PrintText($"{program.ToolName} - {program.ToolDesc}");

		return ToolExitCodes.Success;
	}
}
