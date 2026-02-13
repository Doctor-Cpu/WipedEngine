using System.Globalization;
using Wiped.Shared.CVars;

namespace Wiped.Localization.CVars;

internal sealed partial class LocalizationEngineCVars : BaseEngineCVarModule
{
	[CVarDef]
	public static readonly CVar<CultureInfo> Locale = Define("locale", CultureInfo.CurrentCulture);
}
