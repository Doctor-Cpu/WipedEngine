using Wiped.Shared.Reflection;

namespace Wiped.Shared;

internal static class ModuleRunner
{
	public static void RegisterIoC(IGeneratedModule root)
	{
        var ordered = TopologicalSort(root);
		foreach (var module in ordered)
			module.RegisterIoC();
	}

	public static void RegisterReflection(IGeneratedModule root, ITypeRegistry registry)
	{
        var ordered = TopologicalSort(root);
		foreach (var module in ordered)
			module.RegisterReflection(ref registry);
	}

	private static List<IGeneratedModule> TopologicalSort(IGeneratedModule root)
	{
		var allModules = CollectModules(root);

		// kahns algorithm

		Dictionary<IGeneratedModule, int> indegree = new(allModules.Count);
		Dictionary<IGeneratedModule, List<IGeneratedModule>> dependents = new(allModules.Count);
		foreach (var module in allModules)
		{
			indegree[module] = 0;
			dependents[module] = [];
		}

		foreach (var module in allModules)
		{
			foreach (var dep in module.Dependencies)
			{
				indegree[module]++;
				dependents[dep].Add(module);
			}
		}

		Queue<IGeneratedModule> queue = new(allModules.Count);
		foreach (var (module, degree) in indegree)
		{
			if (degree == 0)
				queue.Enqueue(module);
		}

		List<IGeneratedModule> result = new(allModules.Count);
		while(queue.TryDequeue(out var module))
		{
			result.Add(module);

			foreach (var dep in dependents[module])
			{
				if (--indegree[dep] == 0)
					queue.Enqueue(dep);
			}
		}

#if DEBUG // source generated modules so shouldnt matter
        if (result.Count != allModules.Count)
            throw new InvalidOperationException("Module dependency cycle detected.");
#endif

		return result;
	}

	private static HashSet<IGeneratedModule> CollectModules(IGeneratedModule root)
	{
		HashSet<IGeneratedModule> visited = [];
		Stack<IGeneratedModule> stack = [];
		stack.Push(root);

		while (stack.TryPop(out var current))
		{
			if (!visited.Add(current))
				continue;

			foreach (var dep in current.Dependencies)
				stack.Push(dep);
		}

		return visited;
	}
}
