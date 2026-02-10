using System.Reflection;
using Wiped.Shared.IoC;

namespace Wiped.Shared.Assemblies;

internal interface IAssemblyManager : IManager
{
	IEnumerable<Assembly> GetAssemblies();
    void RegisterAssembly(Assembly asm, AssemblyScope scope);
}
