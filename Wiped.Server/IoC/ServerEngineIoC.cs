using Wiped.Shared.IoC;

namespace Wiped.Server.IoC;

internal static class ServerEngineIoC
{
	internal static void Register()
	{
		SharedEngineIoC.Register();
	}
}
