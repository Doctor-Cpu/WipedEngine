namespace Wiped.Shared.Localisation;

public readonly record struct TextLocParam(string Id, object Data)
{
    public static implicit operator (string id, object data)(TextLocParam locParam)
    {
        return (locParam.Id, locParam.Data);
    }

    public static implicit operator TextLocParam((string Id, object Data) input)
    {
        return new TextLocParam(input.Id, input.Data);
    }

    public override string ToString() => $"{Id} - {Data}";
}
