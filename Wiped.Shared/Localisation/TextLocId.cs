namespace Wiped.Shared.Localisation;

public readonly record struct TextLocId(string Id) : IEquatable<string>, IComparable<TextLocId>
{
    public static implicit operator string(TextLocId locId)
    {
        return locId.Id;
    }

    public static implicit operator TextLocId(string id)
    {
        return new TextLocId(id);
    }

    public static implicit operator TextLocId?(string? id)
    {
        return id == null ? default(TextLocId?) : new TextLocId(id);
    }

    public bool Equals(string? other)
    {
		return string.Equals(Id, other, StringComparison.OrdinalIgnoreCase);
    }

    public int CompareTo(TextLocId other)
    {
        return string.Compare(Id, other.Id, StringComparison.OrdinalIgnoreCase);
    }

    public override string ToString() => Id;
}
