using Wiped.Shared.Localisation;

namespace Wiped.Shared.CVars;

public abstract class BaseCVarModule
{
	protected static CVar<T> Define<T>(string id, T defaultValue, TextLocId? name = null, TextLocId? description = null) where T : notnull
	{
		return new CVar<T>(id, defaultValue, name, description);
	}
}
