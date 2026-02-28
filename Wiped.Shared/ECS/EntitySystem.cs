using Wiped.Shared.IoC;

namespace Wiped.Shared.ECS;

// no IHotReloadable as entity manager will handle it and everything else with it
public abstract partial class EntitySystem
{
	[Dependency] protected readonly IoCDynamic<IEntityManager> EntityManager = default!;

    protected internal virtual void Initialize()
	{
	}

    protected internal virtual void Shutdown()
	{
	}

	protected internal virtual void Update(float frameTime)
	{
	}

	protected internal virtual void FrameUpdate(float frameTime)
	{
	}

	protected void SubscribeLocalEvent<TEvent>(Action<TEvent> handler) where TEvent : notnull
	{
		EntityManager.Value.SubscribeLocalEvent(handler);
	}

	protected void SubscribeLocalEvent<TComp, TEvent>(Action<Entity<TComp>, TEvent> handler) where TComp : Component where TEvent : notnull
	{
		EntityManager.Value.SubscribeLocalEvent(handler);
	}

	protected void RaiseLocalEvent<TEvent>(ref TEvent ev) where TEvent : notnull
	{
		EntityManager.Value.RaiseLocalEvent(ref ev);
	}

	protected void RaiseLocalEvent<TEvent>(EntityUid uid, ref TEvent ev) where TEvent : notnull
	{
		EntityManager.Value.RaiseLocalEvent(uid, ref ev);
	}

	protected void SubscribeNetworkEvent<TEvent>(Action<TEvent> handler) where TEvent : notnull
	{
		EntityManager.Value.SubscribeNetworkEvent(handler);
	}

	protected void RaiseNetworkEvent<TEvent>(ref TEvent ev) where TEvent : notnull
	{
		EntityManager.Value.RaiseNetworkEvent(ref ev);
	}
}
