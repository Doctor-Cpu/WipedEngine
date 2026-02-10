using System.Globalization;
using Wiped.Shared.CVars;

namespace Wiped.Localisation.CVars;

internal sealed partial class LocalisationEngineCVars : BaseEngineCVarModule
{
	[CVarDef]
	public static readonly CVar<CultureInfo> Locale = Define("locale", CultureInfo.CurrentCulture);
}
