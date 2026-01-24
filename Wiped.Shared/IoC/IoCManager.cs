namespace Wiped.Shared.IoC;

public static class IoCManager
{
	internal static readonly IoCContainer EngineContainer = new();
	internal static readonly IoCContainer ContentContainer = new();

	internal static void BindEngine<TInterface, TImpl>() where TImpl : BaseManager, TInterface, new()
	{
		EngineContainer.Bind<TInterface, TImpl>();
	}

	public static void BindContent<TInterface, TImpl>() where TImpl : BaseManager, TInterface, new()
	{
		ContentContainer.Bind<TInterface, TImpl>();
	}

	public static T Resolve<T>()
	{
		if (EngineContainer.TryResolve<T>(out var val))
			return val;

		if (ContentContainer.TryResolve(out val))
			return val;

		throw new InvalidOperationException($"Cannot find resolve type {typeof(T)}");
	}

	public static void ResolveDependencies(object instance)
	{
		EngineContainer.ResolveDependencies(instance);
		ContentContainer.ResolveDependencies(instance, false);
	}

	internal static void FreezeEngine()
	{
		EngineContainer.Freeze();
	}

	internal static void FreezeContent()
	{
		ContentContainer.Freeze();
	}

	internal static void Initialize()
	{
		EngineContainer.Initialize();
		ContentContainer.Initialize();
	}

	internal static void Shutdown()
	{
		EngineContainer.Shutdown();
		ContentContainer.Shutdown();
	}
}
