using Wiped.Shared.Reflection;

namespace Wiped.Shared.Logging.Sink;

[ReflectableBaseUsage]
internal interface ILogSink
{
	void Write(LogEntry entry);
}
