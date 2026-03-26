using Wiped.Shared.CVars;
using Wiped.Shared.IoC;
using Wiped.Shared.Lifecycle;
using Wiped.Shared.Logging.CVars;
using Wiped.Shared.Logging.Sink;
using Wiped.Shared.VFS;

namespace Wiped.Shared.Logging;

[AutoBind(typeof(ILoggingManager))]
internal sealed class LoggingManager : ILoggingManager, IHotReloadable
{
	[Dependency] private readonly IoCDynamic<ICVarManager> _cvar = default!;
	[Dependency] private readonly IoCDynamic<ILifecycleManager> _lifecycle = default!;

	// attempting to keep these down to a minimum
	// makes them an instant no for any logging inside their Initialize()
	public Type[] After => [typeof(ICVarManager), typeof(ILifecycleManager)];

	private LogLevel _minLevel = LogLevel.Debug;
	private IEnumerable<ILogSink> _sinks = []; // throw in a tap to go with the kitchen sink

	public void Initialize()
	{
		_sinks = _lifecycle.Value.GetAll<ILogSink>();
		_minLevel = _cvar.Value.GetValue(LoggingEngineCVars.MinLogLevel);
	}

	public void Log(LogLevel level, string message, DateTime? timestamp = null)
	{
		if (level < _minLevel)
			return;

		timestamp ??= DateTime.UtcNow;

		var entry = new LogEntry(timestamp.Value, level, message);

		foreach (var sink in _sinks)
			sink.Write(entry);
	}

	public void Debug(string message) => Log(LogLevel.Debug, message);
	public void Info(string message) => Log(LogLevel.Info, message);
	public void Warning(string message) => Log(LogLevel.Warning, message);
	public void Error(string message) => Log(LogLevel.Error, message);
}
