using Wiped.Shared.Reflection;

namespace Wiped.Localization.Text.Functions;

[ReflectableBaseUsage]
public interface ITextLocalizationFunction
{
	public string Name { get; }

	public HashSet<CultureInfo>? SupportedCultures { get; }

	public object? Function(CultureInfo culture, List<object?> positionalArgs, Dictionary<string, object?> namedArgs);
}
