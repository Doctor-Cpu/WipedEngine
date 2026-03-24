namespace Wiped.Localization.Text.Functions;

internal sealed class CapitalizeTextLocalizationFunction : ITextLocalizationFunction
{
	public string Name => "CAPITALIZE";

	public HashSet<CultureInfo>? SupportedCultures => null;

	public object? Function(CultureInfo culture, List<object?> positionalArgs, Dictionary<string, object?> namedArgs)
	{
		if (positionalArgs.Count != 1 || positionalArgs[0] is not string arg)
			return null;

		return arg.ToUpper(culture);
	}
}
