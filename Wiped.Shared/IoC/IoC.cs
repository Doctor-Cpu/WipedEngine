namespace Wiped.Shared.IoC;

public static class IoC
{
	private static readonly Dictionary<Type, Type> _bindings = [];

	public static void Bind<TInterface, TImpl>() where TImpl : TInterface
	{
		_bindings[typeof(TInterface)] = typeof(TImpl);
	}

	public static TInterface Resolve<TInterface>()
	{
		if (!_bindings.TryGetValue(typeof(TInterface), out var implType))
			throw new InvalidOperationException($"No registered binding for {typeof(TInterface).FullName}");

		return (TInterface)Activator.CreateInstance(implType)!;
	}
}
