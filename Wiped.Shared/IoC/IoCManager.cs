using System.Diagnostics.CodeAnalysis;

namespace Wiped.Shared.IoC;

public static class IoCManager
{
	internal static readonly IoCContainer EngineContainer = new();
	internal static readonly IoCContainer ContentContainer = new();

	public static bool TryResolve<T>([NotNullWhen(true)] out T? val)
	{
		if (TryResolve(typeof(T), out var tmp))
		{
			val = (T)tmp;
			return true;
		}

		val = default;
		return false;
	}

	public static bool TryResolve(Type type, [NotNullWhen(true)] out object? val)
	{
		if (EngineContainer.TryResolve(type, out val))
			return true;

		if (ContentContainer.TryResolve(type, out val, false))
			return true;

		return false;
	}

	public static object Resolve(Type type)
	{
		if (EngineContainer.TryResolve(type, out var val))
			return val;

		if (ContentContainer.TryResolve(type, out val))
			return val;

		throw new InvalidOperationException($"Cannot find resolve type {type.FullName}");
	}

	public static T Resolve<T>()
	{
		return (T)Resolve(typeof(T));
	}

	internal static IEnumerable<object> GetAllResolved()
	{
		return EngineContainer.GetAllInstances().Concat(ContentContainer.GetAllInstances());
	}

	public static void ResolveDependencies(object instance)
	{
		EngineContainer.InjectInto(instance);
		ContentContainer.InjectInto(instance);
	}

	internal static void EngineTransitionTo(IoCLifecycle next)
	{
		EngineContainer.TransitionTo(next);
	}

	internal static void ContentTransitionTo(IoCLifecycle next)
	{
		ContentContainer.TransitionTo(next);
	}
	
	internal static void AutoBindEngine()
	{
		EngineContainer.AutoBind();
	}

	internal static void AutoBindContent()
	{
		ContentContainer.AutoBind();
	}

	internal static void CreateInstancesEngine()
	{
		EngineContainer.CreateInstances();
	}

	internal static void CreateInstancesContent()
	{
		ContentContainer.CreateInstances();
	}
}
