using Wiped.Shared.CVars;

namespace Wiped.Shared.Logging.CVars;

internal sealed class LoggingEngineCVars : BaseEngineCVarModule
{
	[CVarDef]
	public static readonly CVar<LogLevel> MinLogLevel = Define("log_min", LogLevel.Debug);
}
