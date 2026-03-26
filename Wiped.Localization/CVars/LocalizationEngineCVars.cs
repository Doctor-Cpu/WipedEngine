using Wiped.Shared.CVars;

namespace Wiped.Localization.CVars;

internal sealed class LocalizationEngineCVars : BaseEngineCVarModule
{
	[CVarDef]
	public static readonly CVar<CultureInfo> Locale = Define("locale", CultureInfo.CurrentCulture);
}
