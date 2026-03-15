using Wiped.Shared.Localization;
using Wiped.Shared.Reflection;

namespace Wiped.Tools;

[ReflectableBaseUsage]
public abstract class BaseTool
{
	public abstract string ToolName { get; }
	public abstract TextLocId? ToolDesc { get; }

	public abstract void Start(string[] args);
}
