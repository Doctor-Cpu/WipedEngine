using System.Text;

namespace Wiped.Shared.Localization.Text;

public readonly record struct TextLoc(TextLocId Id, params TextLocParam[] Params) : IEquatable<TextLocId>, IComparable<TextLocId>
{
    public static implicit operator TextLocId(TextLoc loc) => loc.Id;

    public static implicit operator TextLoc(TextLocId id) => new(id);

    public bool Equals(TextLocId other) => Id == other;

    public int CompareTo(TextLocId other) => string.Compare(Id.Id, other.Id, StringComparison.OrdinalIgnoreCase);

    public override string ToString()
	{
		StringBuilder builder = new($"{Id}");

		foreach (var param in Params)
			builder.Append($"( {param} )");

		return builder.ToString();
	}
}
