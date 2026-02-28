namespace Wiped.Shared.ECS;

public abstract class Component
{
	internal EntityUid Owner;
	internal LifeStage LifeStage = LifeStage.Invalid;
}
