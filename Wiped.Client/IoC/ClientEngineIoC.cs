using Wiped.Shared.IoC;

namespace Wiped.Client.IoC;

internal static class ClientEngineIoC
{
	internal static void Register()
	{
		SharedEngineIoC.Register();
	}
}
