using System.Diagnostics.CodeAnalysis;
using Wiped.Shared.IoC;
using Wiped.Shared.Lifecycle;
using Wiped.Shared.VFS;

namespace Wiped.Tools;

[AutoBind(typeof(IProgramManager))]
internal sealed class ProgramManager : IProgramManager, IHotReloadable
{
	[Dependency] private readonly ILifecycleManager _lifecycle = default!;

	public Type[] After => [typeof(ILifecycleManager)];

	private readonly Dictionary<string, BaseTool> _byName = new(StringComparer.OrdinalIgnoreCase);
	private readonly Dictionary<Type, BaseTool> _byType = [];

	public void Initialize()
	{
		foreach (var program in _lifecycle.GetAll<BaseTool>())
		{
			_byName[program.ToolName] = program;
			_byType[program.GetType()] = program;
		}
	}

	public void Shutdown()
	{
		_byName.Clear();
		_byType.Clear();
	}

	public int Load<T>(string[] args) where T : BaseTool
	{
		var type = typeof(T);
		if (!_byType.TryGetValue(type, out var tool))
			throw new InvalidOperationException($"Tried to load tool of type {type} which does not exist");

		return tool.Start(args);
	}

	public int Load(string name, string[] args)
	{
		if (!_byName.TryGetValue(name, out var tool))
			throw new InvalidOperationException($"Tried to load tool {name} which does not exist");

		return tool.Start(args);
	}

	public bool TryGet<T>([NotNullWhen(true)] out T? tool) where T : BaseTool
	{
		var type = typeof(T);
		if (_byType.TryGetValue(type, out var t))
		{
			tool = (T)t;
			return true;
		}

		tool = null;
		return false;
	}

	public bool TryGet(Type type, [NotNullWhen(true)] out BaseTool? tool)
	{
		return _byType.TryGetValue(type, out tool);
	}

	public bool TryGet(string name, [NotNullWhen(true)] out BaseTool? tool)
	{
		return _byName.TryGetValue(name, out tool);
	}

	public IEnumerable<BaseTool> GetAll() => _byType.Values;
}
