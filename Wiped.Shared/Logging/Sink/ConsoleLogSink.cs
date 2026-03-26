namespace Wiped.Shared.Logging.Sink;

internal sealed class ConsoleLogSink : ILogSink
{
	public void Write(LogEntry entry)
	{
		var color = GetLogColor(entry.Level);
		var name = GetLogName(entry.Level);

		var coloredLevel = $"\x1b[1;{color}m{name}\x1b[0m";

		Console.WriteLine($"[{entry.Timestamp:yyyy/MM/dd HH:mm:ss}] [{coloredLevel}] {entry.Message}");
	}

	private static string GetLogColor(LogLevel level) // ansi color code
	{
		return level switch
		{
			LogLevel.Debug => "35", // magenta
			LogLevel.Info => "36", // cyan
			LogLevel.Warning => "33", // yellow
			LogLevel.Error => "31", // red
			_ => "37" // white
		};
	}

	private static string GetLogName(LogLevel level)
	{
		return level switch
		{
			LogLevel.Debug => "DEBUG",
			LogLevel.Info => "INFO",
			LogLevel.Warning => "WARN",
			LogLevel.Error => "ERROR",
			_ => "UNKNOWN",
		};
	}
}
