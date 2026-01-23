namespace Wiped.Shared.Reflection;

public interface IReflectionManager
{
	IEnumerable<Type> GetAllDerivedTypes<TBase>();
}
