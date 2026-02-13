using Linguini.Bundle.Builder;
using Wiped.Localization.CVars;
using Wiped.Shared.CVars;
using Wiped.Shared.IoC;
using Wiped.Shared.VFS;

namespace Wiped.Localization;

[AutoBind(typeof(ITextLocalizationManager))]
internal sealed class TextLocalizationManager : ITextLocalizationManager, IHotReloadable
{
	[Dependency] private readonly ICVarManager _cvar = default!;

	public Type[] After = [typeof(ICVarManager)];

	public void Initialize()
	{
		SetBundler();
	}

	public void Shutdown()
	{
	}

	private void SetBundler()
	{
		var local = _cvar.GetValue(LocalizationEngineCVars.Locale);

		var builder = LinguiniBuilder.Builder();
		builder.CultureInfo(local);
	}

	public string GetString(TextLoc loc)
	{
		return loc.Id;
	}

	public string GetString(TextLocId id, params TextLocParam[] parameters)
	{
		return id;
	}
}
