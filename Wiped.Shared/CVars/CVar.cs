using Wiped.Shared.Localisation;

namespace Wiped.Shared.CVars;

public sealed class CVar<T> : ICVar where T : notnull
{
	public string Id { get; }
	public TextLocId Name { get; }
	public TextLocId Description { get; }

	public T DefaultValue { get; }

    public Type ValueType => typeof(T);
    object ICVar.DefaultUntypedValue => DefaultValue;

	public override string ToString() => $"({Id})";

	internal CVar(string id, T defaultValue, TextLocId? name = null, TextLocId? description = null)
	{
		Id = id;
		Name = name ?? $"cvar-{id}-name";
		Description = description ?? $"cvar-{id}-desc";
		DefaultValue = defaultValue;
	}
}
