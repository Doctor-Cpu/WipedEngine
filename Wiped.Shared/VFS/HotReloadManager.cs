using System.Runtime.CompilerServices;
using System.Text;
using Wiped.Shared.IoC;
using Wiped.Shared.Lifecycle;
using DependencyAttribute = Wiped.Shared.IoC.DependencyAttribute;

namespace Wiped.Shared.VFS;

[AutoBind(typeof(IHotReloadManager))]
internal sealed class HotReloadManager : IHotReloadManager
{
	[Dependency] private readonly IoCDynamic<ILifecycleManager> _lifeCycle = default!;

	public void Initialize()
	{
		var ordered = GetSortedTypes();
		foreach (var system in ordered)
			system.Initialize();
	}

	public void Shutdown()
	{
		var ordered = GetSortedTypes();
		for (var i = ordered.Count - 1; i >= 0; i--) //shutdown in reverse order
			ordered[i].Shutdown();
	}

	public void Reload()
	{
		// just initialize and shutdown merged into one
		// saves calculating the sorting twice
		var ordered = GetSortedTypes();
		foreach (var system in ordered)
			system.Initialize();

		for (var i = ordered.Count - 1; i >= 0; i--)
			ordered[i].Shutdown();
	}

	private List<IHotReloadable> GetSortedTypes()
	{
		var systems = _lifeCycle.Value.GetAll<IHotReloadable>().ToList();
		return TopologicalSort(systems);
	}

	private List<IHotReloadable> TopologicalSort(List<IHotReloadable> systems)
	{
		// kahns algorithm

		Dictionary<Type, IHotReloadable> byType = new(systems.Count);
		Dictionary<Type, HashSet<Type>> edges = new(systems.Count);
		Dictionary<Type, int> indegree = new (systems.Count);
		foreach (var system in systems)
		{
			var type = system.GetType();

			byType[type] = system;
			edges[type] = [];
			indegree[type] = 0;
		}

		foreach (var (from, system) in byType)
		{
			foreach (var after in system.After)
			{
				var registeredAfter = GetRegisteredType(after);

				if (!byType.ContainsKey(registeredAfter) && registeredAfter is not IManager)
					throw new InvalidOperationException($"{from.Name} depends on {registeredAfter.Name}, which is not registered");

				if (edges[registeredAfter].Add(from))
					indegree[from]++;
			}

			foreach (var before in system.Before)
			{
				var registeredBefore = GetRegisteredType(before);

				if (!byType.ContainsKey(registeredBefore) && registeredBefore is not IManager)
					throw new InvalidOperationException($"{from.Name} must run before {registeredBefore.Name}, which is not registered");

				if (edges[from].Add(registeredBefore))
					indegree[before]++;
			}
		}

		List<IHotReloadable> ordered = new(systems.Count);
		Queue<Type> queue = new(indegree.Count);
		foreach (var (type, degree) in indegree)
		{
			if (degree != 0)
				continue;

			queue.Enqueue(type);
		}

		while (queue.TryDequeue(out var type))
		{
			ordered.Add(byType[type]);

			foreach (var next in edges[type])
			{
				indegree[next]--;
				if (indegree[next] == 0)
					queue.Enqueue(next);
			}
		}

#if DEBUG
		if (ordered.Count != systems.Count)
		{
			var message = new StringBuilder("HotReload dependency cycle detected:");
			foreach (var (type, degree) in indegree)
			{
				if (degree > 0)
					message.AppendLine(type.FullName);
			}

			throw new InvalidOperationException(message.ToString());
		}
#endif

		return ordered;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static Type GetRegisteredType(Type baseType)
	{
		// special case for managers
		// otherwise youd have to specify every implementation from every namespace
		// which is impossible when a shared manager depends on a manager with specific client/server/tools implementations
		if (typeof(IManager).IsAssignableFrom(baseType))
		{
			var dynamic = IoCManager.Resolve(baseType);
			return dynamic.ManagerType;
		}

		return baseType;
	}
}
