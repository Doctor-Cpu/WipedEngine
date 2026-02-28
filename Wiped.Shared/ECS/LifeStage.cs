namespace Wiped.Shared.ECS;

public enum LifeStage : byte
{
	Invalid = 0,
	Initializing,
	Initialized,
	Paused,
	Terminating,
	Terminated
}
