namespace Wiped.Shared.VFS;

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
