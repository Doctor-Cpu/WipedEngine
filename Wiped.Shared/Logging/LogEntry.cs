namespace Wiped.Shared.Logging;

internal readonly record struct LogEntry(DateTime Timestamp, LogLevel Level, string Message);
