namespace Wiped.Shared.Localization.Text;

public readonly record struct TextLocParam(string Id, object? Value)
{
    public static implicit operator TextLocParam((string Id, object? Value) tuple) => new(tuple.Id, tuple.Value);

    public override string ToString() => $"{Id}: {Value}";
}
