using System.Text;
using Wiped.Shared.IoC;
using Wiped.Shared.Lifecycle;

namespace Wiped.Shared.VFS;

[AutoBind(typeof(IHotReloadManager))]
internal sealed class HotReloadManager : IManager, IHotReloadManager
{
	[Dependency] private readonly ILifecycleManager _lifeCycle = default!;

	public void Initialize()
	{
		var ordered = GetSortedTypes();
		foreach (var system in ordered)
			system.Initialize();
	}

	public void Shutdown()
	{
		var ordered = GetSortedTypes();
		for (var i = ordered.Count - 1; i <= 0; i--) //shutdown in reverse order
			ordered[i].Shutdown();
	}

	private List<IHotReloadable> GetSortedTypes()
	{
		var systems = _lifeCycle.GetAll<IHotReloadable>().ToList();
		return TopologicalSort(systems);
	}

	private List<IHotReloadable> TopologicalSort(List<IHotReloadable> systems)
	{
		// kahns algorithm

		Dictionary<Type, IHotReloadable> byType = new(systems.Count);
		Dictionary<Type, HashSet<Type>> edges = new(systems.Count);
		Dictionary<Type, int> indegrees = new (systems.Count);
		foreach (var system in systems)
		{
			var type = system.GetType();
			byType[type] = system;
			edges[type] = [];
			indegrees[type] = 0;
		}

		foreach (var system in systems)
		{
			var from = system.GetType();

			foreach (var after in system.After)
			{
				if (!byType.ContainsKey(after))
					throw new InvalidOperationException($"{from.Name} depends on {after.Name}, which is not registered");

				if (edges[after].Add(from))
					indegrees[from]++;
			}

			foreach (var before in system.Before)
			{
				if (!byType.ContainsKey(before))
					throw new InvalidOperationException($"{from.Name} must run before {before.Name}, which is not registered");

				if (edges[from].Add(before))
					indegrees[before]++;
			}
		}

		List<IHotReloadable> ordered = new(systems.Count);
		Queue<Type> queue = new(indegrees.Count);
		foreach (var (type, indegree) in indegrees)
		{
			if (indegree != 0)
				continue;

			queue.Enqueue(type);
		}

		while (queue.TryDequeue(out var type))
		{
			ordered.Add(byType[type]);

			foreach (var next in edges[type])
			{
				indegrees[next]--;
				if (indegrees[next] == 0)
					queue.Enqueue(next);
			}
		}

#if DEBUG

		if (ordered.Count != systems.Count)
		{
			var message = new StringBuilder("HotReload dependency cycle detected:");
			foreach (var (type, indegree) in indegrees)
			{
				if (indegree > 0)
					message.AppendLine(type.FullName);
			}

			throw new InvalidOperationException(message.ToString());
		}
#endif

		return ordered;
	}
}
