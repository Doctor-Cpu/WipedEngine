using Wiped.Shared.IoC;
using Wiped.Shared.Localisation;

namespace Wiped.Localisation;

public interface ITextLocalisationManager : IManager
{
	string GetString(TextLoc loc);
	string GetString(TextLocId id, params TextLocParam[] parameters);
}
