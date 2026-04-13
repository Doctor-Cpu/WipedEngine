using Wiped.Shared.CVars;

namespace Wiped.Tools.CVars;

internal sealed class ProgramLauncehrEngineCVars : BaseEngineCVarModule
{
	[CVarDef]
	public static readonly CVar<int> HistoryMaxLength = Define("program_history_length", 1024);
}
