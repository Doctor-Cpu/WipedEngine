using System.Diagnostics.CodeAnalysis;
using Wiped.Shared.IoC;
using Wiped.Shared.VFS;

namespace Wiped.Shared.ECS;

[AutoBind(typeof(IEntityManager), typeof(IEngineEntityManager))]
internal sealed class EntityManager : IEntityManager, IEngineEntityManager, IHotReloadable
{
    private readonly Dictionary<Type, List<Delegate>> _localEventSubscribers = [];
    private readonly Dictionary<Type, Dictionary<Type, Delegate>> _componentEventSubscribers = [];

	private readonly Dictionary<EntityUid, List<Component>> _components = [];

    private readonly Dictionary<Type, (EntitySystem System, int Priority)> _systems = [];

#region Subscriptions

	public void SubscribeLocalEvent<TEvent>(Action<TEvent> handler) where TEvent : notnull
	{
		var type = typeof(TEvent);
		if (!_localEventSubscribers.TryGetValue(type, out var list))
			_localEventSubscribers[type] = list = [];
		list.Add(handler);
	}

	public void SubscribeLocalEvent<TComp, TEvent>(Action<Entity<TComp>, TEvent> handler)
		where TComp : Component
		where TEvent : notnull
	{
        var eventType = typeof(TEvent);
		var compType = typeof(TComp);

        if (!_componentEventSubscribers.TryGetValue(eventType, out var compSubscribers))
            _componentEventSubscribers[eventType] = compSubscribers = [];

#if DEBUG
		if (compSubscribers.ContainsKey(compType))
			throw new InvalidOperationException($"Multiple subscribers for {eventType} with {compType}");
#endif

        compSubscribers[compType] = handler;
	}

	public void RaiseLocalEvent<TEvent>(ref TEvent ev) where TEvent : notnull
	{
        var type = typeof(TEvent);
        if (_localEventSubscribers.TryGetValue(type, out var subscribers))
        {
			foreach (var subscriber in subscribers)
				subscriber.DynamicInvoke(ev);
        }
	}

	public void RaiseLocalEvent<TEvent>(EntityUid uid, ref TEvent ev) where TEvent : notnull
	{
		if (!_components.TryGetValue(uid, out var comps))
			return;

		var eventType = typeof(TEvent);

		if (!_componentEventSubscribers.TryGetValue(eventType, out var compSubscribers))
			return;

		foreach (var comp in comps)
		{
			var compType = comp.GetType();

			if (!compSubscribers.TryGetValue(compType, out var subscriber))
				continue;

			var entity = Activator.CreateInstance(
				typeof(Entity<>).MakeGenericType(compType),
				uid,
				comp
			);
			subscriber.DynamicInvoke(entity, ev);
		}
	}

	public void SubscribeNetworkEvent<TEvent>(Action<TEvent> handler) where TEvent : notnull
	{
		throw new NotImplementedException();
	}

	public void RaiseNetworkEvent<TEvent>(ref TEvent ev) where TEvent : notnull
	{
		throw new NotImplementedException();
	}

#endregion

#region Component Fetching

	public TComp GetComp<TComp>(EntityUid uid) where TComp : Component
	{
		if (!_components.TryGetValue(uid, out var comps))
			throw new InvalidOperationException($"Entity {uid.Id} does not exist");

		foreach (var c in comps)
		{
			if (c is TComp comp)
				return comp;
		}

		throw new InvalidOperationException($"Entity {uid.Id} does not have component {typeof(TComp).Name}");
	}

	public bool TryGetComp<TComp>(EntityUid uid, [NotNullWhen(true)] out TComp comp) where TComp : Component
	{
		comp = default!;
		if (!_components.TryGetValue(uid, out var comps))
			return false;

		foreach (var c in comps)
		{
			if (c is not TComp matched)
				continue;

			comp = matched;
			return true;
		}

		return false;
	}

#endregion

	public void RegisterSystem(EntitySystem system, int priority = 0)
	{
		var type = system.GetType();

		if (_systems.TryGetValue(type, out var existing))
		{
			if (priority < existing.Priority)
				return;
		}
		_systems[type] = (system, priority);
	}

	public void Initialize()
	{
		foreach (var (sys, _) in _systems.Values)
			sys.Initialize();
	}

	public void Shutdown()
	{
		foreach (var (sys, _) in _systems.Values)
			sys.Shutdown();

		_localEventSubscribers.Clear();
		_componentEventSubscribers.Clear();
		_components.Clear();
		_systems.Clear();
	}

	private void Update(float frameTime)
	{
		foreach (var (sys, _) in _systems.Values)
			sys.Update(frameTime);
	}

	private void FrameUpdate(float frameTime)
	{
		foreach (var (sys, _) in _systems.Values)
			sys.FrameUpdate(frameTime);
	}
}
