using Wiped.Shared.IoC;

namespace Wiped.Shared.Lifecycle;

internal interface ILifecycleManager : IManager
{
    T Get<T>() where T : notnull;
    object Get(Type type);

    IEnumerable<T> GetAll<T>();
}
