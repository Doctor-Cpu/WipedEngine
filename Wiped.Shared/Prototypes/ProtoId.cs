namespace Wiped.Shared.Prototypes;

public readonly record struct ProtoId<TProto>(string Id) where TProto: IPrototype
{
    public override string ToString() => Id;
}
