using Wiped.Shared.CVars;
using Wiped.Shared.ECS;
using Wiped.Shared.Instance;
using Wiped.Shared.Prototypes;
using Wiped.Shared.Reflection;
using Wiped.Shared.Serialization;
using Wiped.Shared.VFS;

namespace Wiped.Shared.IoC;

internal static class SharedEngineIoC
{
	internal static void Register()
	{
		IoCManager.BindEngine<IContentVFSManager, ContentVFSManager>();
		IoCManager.BindEngine<IEngineContentVFSManager, ContentVFSManager>();
		IoCManager.BindEngine<ICVarManager, CVarManager>();
		IoCManager.BindEngine<IDataDefinitionRegistryManager, DataDefinitionRegistryManager>();
		IoCManager.BindEngine<IEntityManager, EntityManager>();
		IoCManager.BindEngine<IEngineEntityManager, EntityManager>();
		IoCManager.BindEngine<IInstanceManager, InstanceManager>();
		IoCManager.BindEngine<IPrototypeManager, PrototypeManager>();
		IoCManager.BindEngine<IEnginePrototypeManager, PrototypeManager>();
		IoCManager.BindEngine<IReflectionManager, ReflectionManager>();
		IoCManager.BindEngine<IYamlManager, YamlManager>();
	}
}
