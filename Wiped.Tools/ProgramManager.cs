using System.Diagnostics.CodeAnalysis;
using System.Text;
using Wiped.Localization.Text;
using Wiped.Shared.CVars;
using Wiped.Shared.IoC;
using Wiped.Shared.Lifecycle;
using Wiped.Shared.Localization.Text;
using Wiped.Shared.VFS;
using Wiped.Tools.CVars;

namespace Wiped.Tools;

[AutoBind(typeof(IProgramManager), typeof(IEngineProgramManager))]
internal sealed class ProgramManager : IProgramManager, IEngineProgramManager, IHotReloadable
{
	[Dependency] private readonly IoCDynamic<ICVarManager> _cVar = default!;
	[Dependency] private readonly IoCDynamic<IEngineContentVFSManager> _engineVfs = default!;
	[Dependency] private readonly IoCDynamic<ILifecycleManager> _lifecycle = default!;
	[Dependency] private readonly IoCDynamic<ITextLocalizationManager> _textLocalization = default!;

	public Type[] After => [typeof(ICVarManager), typeof(ILifecycleManager)];

	private readonly Dictionary<string, BaseTool> _byName = new(StringComparer.OrdinalIgnoreCase);
	private readonly Dictionary<Type, BaseTool> _byType = [];

	private string? _toRun;

	private ProgramLauncherHistory _history = new(0);
	private static readonly ContentPath HistoryFilePath =  new("tool-history");

	private static readonly TextLocId LauncherPrompt = "tool-launcher-prompt";
	private static readonly TextLocId LauncherUnknownTool = "tool-generic-error-cli-unknown-tool";

	public void Initialize()
	{
		foreach (var program in _lifecycle.Value.GetAll<BaseTool>())
		{
			_byName[program.ToolName] = program;
			_byType[program.GetType()] = program;
		}

		var size = _cVar.Value.GetValue(ProgramLauncehrEngineCVars.HistoryMaxLength);
		_history = new ProgramLauncherHistory(size);

		if (_engineVfs.Value.TryGetFile(HistoryFilePath, ContentLocation.Data, out var file))
		{
			try
			{
				using var reader = new StreamReader(file);
				var content = reader.ReadToEnd();

				var entries = content.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
				_history.Add(entries);
			}
			finally
			{
				file.Close();
			}
		}
	}

	public void Shutdown()
	{
		using var file = _engineVfs.Value.Write(HistoryFilePath, ContentLocation.Data);
		try
		{
			using var writer = new StreamWriter(file);
			foreach (var entry in _history.GetEntries())
				writer.WriteLine(entry);
		}
		finally
		{
			file.Close();
		}

		_byName.Clear();
		_byType.Clear();

		_history.Clear();
	}

	public void RunLauncher()
	{
		Console.Clear();

		if (_toRun is { } toRun)
		{
			Load(toRun, []);
			return;
		}

		Load<IntroTool>([]); // so people arent utterly clueless

		var prompt = _textLocalization.Value.GetString(LauncherPrompt);
		while (true)
		{
			Console.Write(prompt);
			var input = ReadLineWithHistory();
			Console.WriteLine();

			if (string.IsNullOrWhiteSpace(input))
				continue;

			_history.Add(input);

			var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			if (!TryGet(parts[0], out var tool))
			{
				var error = _textLocalization.Value.GetString(LauncherUnknownTool, ("name", parts[0]));
				Console.WriteLine(error);
				continue;
			}

			tool.Start(parts[1..parts.Length]);
		}
	}

	private string ReadLineWithHistory()
	{
		var buffer = new StringBuilder();
		var historyIndex = -1; // new command so is newer than the latest stored

		var startLeft = Console.CursorLeft;
		var startTop = Console.CursorTop;


		var cursorIndex = 0;

		while (true)
		{
			var key = Console.ReadKey(true);

			switch (key.Key)
			{
				case ConsoleKey.Enter:
					return buffer.ToString();

				case ConsoleKey.Backspace:
					if (buffer.Length > 0)
					{
						buffer.Remove(cursorIndex - 1, 1);
						cursorIndex--;
					}

					break;

				case ConsoleKey.Delete:
					if (cursorIndex < buffer.Length)
						buffer.Remove(cursorIndex, 1);

					break;

				case ConsoleKey.LeftArrow:
					if (cursorIndex > 0)
						cursorIndex--;

					break;

				case ConsoleKey.RightArrow:
					if (cursorIndex < buffer.Length)
						cursorIndex++;

					break;

				case ConsoleKey.UpArrow:
					if (_history.IsValidIndex(historyIndex + 1))
					{
						historyIndex++;
						buffer = new(_history.PeekIndex(historyIndex));
						cursorIndex = buffer.Length;
					}
					break;

				case ConsoleKey.DownArrow:
					if (_history.IsValidIndex(historyIndex - 1))
					{
						historyIndex--;
						buffer = new(_history.PeekIndex(historyIndex));
						cursorIndex = buffer.Length;
					}
					break;

				default:
					if (!char.IsControl(key.KeyChar))
					{
						buffer.Insert(cursorIndex, key.KeyChar);
						cursorIndex++;
					}

					break;
			}

			// clear line
			Console.SetCursorPosition(startLeft, startTop);
			Console.Write(new string(' ', Console.WindowWidth));

			// rewrite buffer
			Console.SetCursorPosition(startLeft, startTop);
			Console.Write(buffer.ToString());

			// move cursor to correct position
			Console.SetCursorPosition(startLeft + cursorIndex, startTop);
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
