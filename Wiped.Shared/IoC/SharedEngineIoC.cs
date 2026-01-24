using Wiped.Shared.Instance;
using Wiped.Shared.Prototypes;
using Wiped.Shared.Reflection;
using Wiped.Shared.Serialization;

namespace Wiped.Shared.IoC;

internal static class SharedEngineIoC
{
	internal static void Register()
	{
		IoCManager.BindEngine<IDataDefinitionRegistryManager, DataDefinitionRegistryManager>();
		IoCManager.BindEngine<IInstanceManager, InstanceManager>();
		IoCManager.BindEngine<IEnginePrototypeManager, PrototypeManager>();
		IoCManager.BindEngine<IReflectionManager, ReflectionManager>();
		IoCManager.BindEngine<IYamlManager, YamlManager>();
	}
}
