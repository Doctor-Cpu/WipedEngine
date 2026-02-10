using Wiped.Shared.IoC;

namespace Wiped.Shared.CVars;

public interface ICVarManager : IManager
{
	T GetValue<T>(CVar<T> cvar) where T : notnull;
	void SetValue<T>(CVar<T> cVar, T value) where T : notnull;
}
