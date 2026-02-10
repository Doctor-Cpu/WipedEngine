using System.Text;

namespace Wiped.Shared.Localisation;

public readonly record struct TextLoc(TextLocId Id, params TextLocParam[] Params) : IEquatable<TextLocId>, IComparable<TextLocId>
{
    public static implicit operator TextLocId(TextLoc loc)
    {
        return loc.Id;
    }

    public static implicit operator TextLoc(TextLocId id)
    {
        return new TextLoc(id);
    }

    public bool Equals(TextLocId other)
	{
        return Id == other;
	}

    public int CompareTo(TextLocId other)
    {
        return string.Compare(Id, other.Id, StringComparison.OrdinalIgnoreCase);
    }

    public override string ToString()
	{
		StringBuilder builder = new($"{Id}");

		foreach (var param in Params)
			builder.Append($"( {param} )");

		return builder.ToString();
	}
}
