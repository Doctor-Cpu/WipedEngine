namespace Wiped.Shared.Instance;

public interface IInstanceManager
{
	T GetOrCreate<T>() where T : class;
	IEnumerable<T> GetAll<T>() where T : class;
}
