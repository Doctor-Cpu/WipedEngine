namespace Wiped.Shared.Prototypes;

public interface IPrototype
{
	string Id { get; }
	bool Abstract { get; }
	IReadOnlyList<ProtoId<IPrototype>> Parents { get; }
}
