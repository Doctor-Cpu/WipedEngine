using System.Diagnostics.CodeAnalysis;
using Wiped.Shared.IoC;
using Wiped.Shared.Localization.Text;

namespace Wiped.Localization.Text;

public interface ITextLocalizationManager : IManager
{
	string GetString(TextLoc loc);
	string GetString(TextLocId id, params TextLocParam[] parameters);

	bool TryGetString(TextLoc loc, [NotNullWhen(true)] out string? message);
	bool TryGetString(TextLocId id, [NotNullWhen(true)] out string? message, params TextLocParam[] parameters);
}
