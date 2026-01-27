namespace Wiped.Shared.Lifecycle;

internal interface ILifecycleManager
{
    T Get<T>() where T : notnull;
    object Get(Type type);

    IEnumerable<T> GetAll<T>();
}
