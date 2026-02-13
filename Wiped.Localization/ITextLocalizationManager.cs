using Wiped.Shared.IoC;
using Wiped.Shared.Localization;

namespace Wiped.Localization;

public interface ITextLocalizationManager : IManager
{
	string GetString(TextLoc loc);
	string GetString(TextLocId id, params TextLocParam[] parameters);
}
