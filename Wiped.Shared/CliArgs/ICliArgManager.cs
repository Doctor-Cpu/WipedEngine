using Wiped.Shared.IoC;

namespace Wiped.Shared.CliArgs;

internal interface ICliArgManager : IManager
{
	void UseArgs(string[] args);
}
