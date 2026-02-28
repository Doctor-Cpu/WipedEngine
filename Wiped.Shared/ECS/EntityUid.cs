namespace Wiped.Shared.ECS;

public struct EntityUid(int id)
{
	public readonly int Id = id;
	internal LifeStage LifeStage = LifeStage.Invalid;
}
