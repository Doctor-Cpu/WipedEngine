using Wiped.Localization.Text;
using Wiped.Shared.IoC;
using Wiped.Shared.Localization.Text;

namespace Wiped.Tools;

public sealed class IntroTool : BaseCliTool
{
	[Dependency] private readonly IoCDynamic<ITextLocalizationManager> _textLocalization = default!;

	public override string ToolName => "intro";

	private static readonly TextLocId IntroText = "tool-intro-intro";

	protected override void Run(string[] args)
	{
		var text = _textLocalization.Value.GetString(IntroText);
		PrintText(text);
	}
}
