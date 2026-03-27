using Wiped.Shared.Reflection;

namespace Wiped.Shared.VFS;

[ReflectableBaseUsage]
public interface IHotReloadable
{
	Type[] Before => [];
	Type[] After => [];

	void Initialize()
	{
	}

	void Shutdown()
	{
	}
}
