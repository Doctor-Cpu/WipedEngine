using Wiped.Shared.IoC;
using Wiped.Shared.Localisation;

namespace Wiped.Tools;

internal abstract class BaseTool
{
	public abstract string ToolName { get; }
	public abstract TextLocId? ToolDesc { get; }

	public abstract int Start(string[] args);

	internal BaseTool()
	{
		IoCManager.ResolveDependencies(this);
	}
}
