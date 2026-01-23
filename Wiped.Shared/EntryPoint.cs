using Wiped.Shared.IoC;

namespace Wiped.Shared;

internal static class EntryPoint
{
	internal static void Start()
	{
		IoCManager.FreezeEngine();
		IoCManager.ContentContainer.Import(IoCManager.EngineContainer);

		IoCManager.FreezeContent();
	}
}
