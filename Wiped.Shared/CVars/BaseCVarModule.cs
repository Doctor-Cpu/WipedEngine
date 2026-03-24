using Wiped.Shared.Localization.Text;
using Wiped.Shared.Reflection;

namespace Wiped.Shared.CVars;

[ReflectableBaseUsage]
public abstract class BaseCVarModule
{
	protected static CVar<T> Define<T>(string id, T defaultValue, TextLocId? name = null, TextLocId? description = null) where T : notnull => new(id, defaultValue, name, description);
}
