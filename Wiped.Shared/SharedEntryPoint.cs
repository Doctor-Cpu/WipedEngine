using Wiped.Shared.IoC;
using Wiped.Shared.Serialization;
using Wiped.Shared.VFS;

namespace Wiped.Shared;

internal static class SharedEntryPoint
{
	internal static void Start()
	{
		IoCManager.FreezeEngine();
		IoCManager.ContentContainer.Import(IoCManager.EngineContainer);

		var dataDefRegistry = IoCManager.Resolve<IDataDefinitionRegistryManager>();
		dataDefRegistry.Initialize();
	}

	internal static void Initialize()
	{
		var vfs = IoCManager.Resolve<IEngineContentVFSManager>();
		vfs.LoadContent();

		IoCManager.FreezeContent();
	}
}
