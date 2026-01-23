using Wiped.Shared.Reflection;

namespace Wiped.Shared.IoC;

internal static class SharedEngineIoC
{
	internal static void Register()
	{
		IoCManager.BindEngine<IReflectionManager, ReflectionManager>();
	}
}
