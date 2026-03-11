using Linguini.Bundle.Builder;
using Wiped.Localization.CVars;
using Wiped.Shared.CVars;
using Wiped.Shared.IoC;
using Wiped.Shared.VFS;

namespace Wiped.Localization;

[AutoBind(typeof(ITextLocalizationManager))]
internal sealed class TextLocalizationManager : ITextLocalizationManager, IHotReloadable
{
	[Dependency] private readonly IoCDynamic<ICVarManager> _cvar = default!;
	[Dependency] private readonly IoCDynamic<IContentVFSManager> _vfs = default!;

	public Type[] After => [typeof(ICVarManager)];

	public void Initialize()
	{
		SetBundler();
	}

	public void Shutdown()
	{
	}

	private void SetBundler(CultureInfo? local = null)
	{
		local ??= _cvar.Value.GetValue(LocalizationEngineCVars.Locale);

		var builder = LinguiniBuilder.Builder()
		.CultureInfo(local);

		foreach (var path in _vfs.Value.Enumerate(SharedTextLocalizationHelpers.TextPath))
		{
		}
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
