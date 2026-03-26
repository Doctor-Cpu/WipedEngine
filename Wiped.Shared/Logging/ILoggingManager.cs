using Wiped.Shared.IoC;

namespace Wiped.Shared.Logging;

public interface ILoggingManager : IManager
{
	void Log(LogLevel level, string message, DateTime? timestamp = null);

	void Debug(string message);
	void Info(string message);
	void Warning(string message);
	void Error(string message);
}
