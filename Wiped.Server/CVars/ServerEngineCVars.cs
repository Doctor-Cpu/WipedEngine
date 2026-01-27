using Wiped.Shared.CVars;

namespace Wiped.Server.CVars;

internal static partial class ServerEngineCVars
{
	internal static void RegisterAll(ICVarManager manager)
	{
		SharedEngineCVars.RegisterAll(manager);
	}
}
