using Wiped.Shared.IoC;

namespace Wiped.Localization;

public interface ITextLocalizationManager : IManager
{
	string GetString(TextLoc loc);
	string GetString(TextLocId id, params TextLocParam[] parameters);
}
