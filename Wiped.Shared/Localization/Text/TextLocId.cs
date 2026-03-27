namespace Wiped.Shared.Localization.Text;

public readonly record struct TextLocId(string Id) : IEquatable<TextLocId>, IComparable<TextLocId>
{
    public static implicit operator TextLocId(string id) => new(id);
    public static implicit operator TextLocId?(string? id) => id == null ? null : new(id);

    public int CompareTo(TextLocId other)
    {
        return string.Compare(Id, other.Id, StringComparison.OrdinalIgnoreCase);
    }

    public override string ToString() => Id;

	public override int GetHashCode() => Id.GetHashCode();
}
