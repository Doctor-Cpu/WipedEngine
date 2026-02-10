using Wiped.Localisation.CVars;
using Wiped.Shared.CVars;
using Wiped.Shared.IoC;
using Wiped.Shared.Localisation;

namespace Wiped.Localisation;

[AutoBind(typeof(ITextLocalisationManager))]
internal sealed class TextLocalisationManager : ITextLocalisationManager
{
	[Dependency] private readonly ICVarManager _cvar = default!;

	public string GetString(TextLoc loc)
	{
		var local = _cvar.GetValue(LocalisationEngineCVars.Locale);
		return loc.Id;
	}

	public string GetString(TextLocId id, params TextLocParam[] parameters)
	{
		var local = _cvar.GetValue(LocalisationEngineCVars.Locale);
		return id;
	}
}
