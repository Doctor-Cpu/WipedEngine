using Wiped.Shared.Reflection;

namespace Wiped.Shared;

public interface IGeneratedModule
{
	IReadOnlyList<IGeneratedModule> Dependencies => [];

	abstract void RegisterIoC();
	//abstract void RegisterReflection(ITypeRegistry registry);
}
