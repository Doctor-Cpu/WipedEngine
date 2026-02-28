using System.Diagnostics.CodeAnalysis;
using Wiped.Shared.IoC;
using Wiped.Shared.Lifecycle;
using Wiped.Shared.VFS;

namespace Wiped.Tools;

[AutoBind(typeof(IProgramManager), typeof(IEngineProgramManager))]
internal sealed class ProgramManager : IProgramManager, IEngineProgramManager, IHotReloadable
{
	[Dependency] private readonly IoCDynamic<ILifecycleManager> _lifecycle = default!;

	public Type[] After => [typeof(ILifecycleManager)];

	private readonly Dictionary<string, BaseTool> _byName = new(StringComparer.OrdinalIgnoreCase);
	private readonly Dictionary<Type, BaseTool> _byType = [];

	private string? _toRun;

	private const string LauncherIntro = """
	Welcome to the tool launcher.
	Launch tools with the program name followed by any arguments.
	For a list of commands use the "help" command.
	For info on what a command does use "help" followed by the command name.
	""";

	private const string LauncherPrompt = "> ";

	public void Initialize()
	{
		foreach (var program in _lifecycle.Value.GetAll<BaseTool>())
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

	public void RunLauncher()
	{
		Console.Clear();
		
		if (_toRun is { } toRun)
		{
			Load(toRun, []);
			return;
		}


		Console.WriteLine(LauncherIntro);
		while (true)
		{
			Console.Write(LauncherPrompt);
			var input = Console.ReadLine();

			if (string.IsNullOrWhiteSpace(input))
				continue;

			var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			if (!TryGet(parts[0], out var tool))
			{
				Console.WriteLine($"Unrecognised tool {parts[0]}");
				continue;
			}

			tool.Start(parts[1..parts.Length]);
		}
	}

	public void SetProgram(string program)
	{
		_toRun = program;
	}

	public void Load<T>(string[] args) where T : BaseTool
	{
		var type = typeof(T);
		if (!_byType.TryGetValue(type, out var tool))
			throw new InvalidOperationException($"Tried to load tool of type {type} which does not exist");

		tool.Start(args);
	}

	public void Load(string name, string[] args)
	{
		if (!_byName.TryGetValue(name, out var tool))
			throw new InvalidOperationException($"Tried to load tool {name} which does not exist");

		tool.Start(args);
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
