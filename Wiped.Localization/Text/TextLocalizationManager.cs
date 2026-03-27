using Linguini.Bundle;
using Linguini.Bundle.Builder;
using Linguini.Shared.Types.Bundle;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Wiped.Localization.CVars;
using Wiped.Localization.Text.Functions;
using Wiped.Shared.CVars;
using Wiped.Shared.IoC;
using Wiped.Shared.Lifecycle;
using Wiped.Shared.Localization.Text;
using Wiped.Shared.VFS;
using Dependency = Wiped.Shared.IoC.DependencyAttribute;

namespace Wiped.Localization.Text;

[AutoBind(typeof(ITextLocalizationManager))]
internal sealed class TextLocalizationManager : ITextLocalizationManager, IHotReloadable
{
	[Dependency] private readonly IoCDynamic<ICVarManager> _cvar = default!;
	[Dependency] private readonly IoCDynamic<ILifecycleManager> _lifecycle = default!;
	[Dependency] private readonly IoCDynamic<IContentVFSManager> _vfs = default!;
	[Dependency] private readonly IoCDynamic<IEngineContentVFSManager> _engineVfs = default!;

	public Type[] After => [typeof(ICVarManager), typeof(ILifecycleManager)];

	private readonly Dictionary<string, FluentBundle> _availableBundles = [];
	private FluentBundle _bundler = default!;

	public void Initialize()
	{
		var functions = _lifecycle.Value.GetAll<ITextLocalizationFunction>().ToArray();

		foreach (var culturePath in _vfs.Value.EnumerateDirectories(SharedLocalizationHelpers.RootPath, false))
		{
			var textCulture = culturePath + SharedTextLocalizationHelpers.TextPath;

			var cultureName = culturePath.GetFileName();
			var culture = CultureInfo.GetCultureInfo(cultureName);

			var builder = LinguiniBuilder.Builder()
			.CultureInfo(culture)
			.AddFiles(_engineVfs.Value.EnumerateAbsolute(textCulture, true));

			foreach (var func in functions)
			{
				if (func.SupportedCultures is { } supportedCultures)
				{
					if (!supportedCultures.Contains(culture))
						continue;
				}

				builder = builder.AddFunction(func.Name, (positional, named) => FluentFunctionWrapper(func, culture, positional, named));
			}

			var bundler = builder.UncheckedBuild();
			_availableBundles[culture.Name] = bundler;
		}

		SetBundler();
	}

	public void Shutdown()
	{
		_availableBundles.Clear();
		_bundler = default!;
	}

	private void SetBundler(CultureInfo? locale = null)
	{
		locale ??= _cvar.Value.GetValue(LocalizationEngineCVars.Locale);

		if (!_availableBundles.TryGetValue(locale.Name, out var bundler))
			throw new InvalidOperationException($"Unknown locale: {locale}");

		_bundler = bundler;
	}

	public bool TryGetString(TextLoc loc, [NotNullWhen(true)] out string? message) => TryGetString(loc.Id, out message, loc.Params);

	public bool TryGetString(TextLocId id, [NotNullWhen(true)] out string? message, params TextLocParam[] parameters)
	{
		var args = BuildArgs(parameters);
		if (_bundler.TryGetAttrMessage(id.Id, args, out var errors, out message))
			return true;

		// TODO: report errors
		return false;
	}

	public string GetString(TextLoc loc)
	{
		return TryGetString(loc, out var message) ? message : loc.Id.Id;
	}

	public string GetString(TextLocId id, params TextLocParam[] parameters)
	{
		return TryGetString(id, out var message, parameters) ? message : id.Id;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private Dictionary<string, IFluentType> BuildArgs(TextLocParam[] parameters)
	{
		Dictionary<string, IFluentType> dict = new(parameters.Length);
		foreach (var param in parameters)
			dict[param.Id] = ToFluentType(param.Value);

		return dict;
	}

	private IFluentType FluentFunctionWrapper(ITextLocalizationFunction func, CultureInfo culture, IList<IFluentType> positionalArgs, IDictionary<string, IFluentType> namedArgs)
	{
		List<object?> safePositional = new(positionalArgs.Count);
		Dictionary<string, object?> safeNamed = new(positionalArgs.Count);

		foreach (var arg in positionalArgs)
			safePositional.Add(FromFluentType(arg));

		foreach (var (key, type) in namedArgs)
			safeNamed[key] = FromFluentType(type);

		var output = func.Function(culture, safePositional, safeNamed);
		return ToFluentType(output);
	}

	private IFluentType ToFluentType(object? value)
	{
		return value switch
		{
			// primitives
			null => FluentNone.None,
			string s => (FluentString)s,
			int i => (FluentNumber)i,
			float f => (FluentNumber)f,
			double d => (FluentNumber)d,
            bool b => (FluentString)b.ToString().ToLowerInvariant(),
            Enum e => (FluentString)e.ToString().ToLowerInvariant(),

			// nested localization
			TextLoc loc => (FluentString)GetString(loc),
			TextLocId id => (FluentString)GetString(id),

			_ => throw new InvalidOperationException($"{value.GetType()} is not a supported type for localization. Consider using fluent functions")
		};
	}

	private object? FromFluentType(IFluentType value)
	{
		return value switch
		{
			FluentNone => null,
			FluentString s => (string)s,
			FluentNumber n => (double)n,
			_ => value.ToString()
		};
	}
}
