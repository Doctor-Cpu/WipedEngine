using System.Diagnostics.CodeAnalysis;
using Wiped.Shared.IoC;

namespace Wiped.Shared.ECS;

public interface IEntityManager : IManager
{
#region Subscriptions
	void SubscribeLocalEvent<TEvent>(Action<TEvent> handler) where TEvent : notnull;

	void SubscribeLocalEvent<TComp, TEvent>(Action<Entity<TComp>, TEvent> handler) where TComp : Component where TEvent : notnull;

	void RaiseLocalEvent<TEvent>(ref TEvent ev) where TEvent : notnull;

	void RaiseLocalEvent<TEvent>(EntityUid uid, ref TEvent ev) where TEvent : notnull;

	void SubscribeNetworkEvent<TEvent>(Action<TEvent> handler) where TEvent : notnull;

	void RaiseNetworkEvent<TEvent>(ref TEvent ev) where TEvent : notnull;

#endregion

#region Component Fetching

	TComp GetComp<TComp>(EntityUid uid) where TComp : Component;

	bool TryGetComp<TComp>(EntityUid uid, [NotNullWhen(true)] out TComp comp) where TComp : Component;

#endregion
}
