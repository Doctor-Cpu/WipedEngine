using Wiped.Shared.Localization;

namespace Wiped.Shared.CVars;

internal interface ICVar
{
	public string Id { get; }
	public TextLocId Name { get; }
	public TextLocId Description { get; }

    Type ValueType { get; }
    object DefaultUntypedValue { get; }
}
