using Wiped.Shared.Localization.Text;
using Wiped.Shared.Reflection;

namespace Wiped.Tools;

[ReflectableBaseUsage]
public abstract class BaseTool
{
	public abstract string ToolName { get; }
	public virtual TextLocId? ToolDesc => $"tool-{ToolName}-desc";

	public abstract void Start(string[] args);
}
