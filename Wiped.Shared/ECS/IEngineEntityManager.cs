using Wiped.Shared.IoC;

namespace Wiped.Shared.ECS;

internal interface IEngineEntityManager : IManager
{
	void RegisterSystem(EntitySystem system, int priority = 0);
}
