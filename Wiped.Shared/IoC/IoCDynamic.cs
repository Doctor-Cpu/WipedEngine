namespace Wiped.Shared.IoC;

public sealed class IoCDynamic<T> : BaseIoCDynamic where T : IManager
{
	public T Value => (T)ResolveUntyped();

    internal override Type ManagerType => typeof(T);

	public static implicit operator T (IoCDynamic<T> dynamic) => dynamic.Value;

	public IoCDynamic(IManager initial) : base(initial)
	{
	}

	public IoCDynamic(T initial) : base(initial)
	{
	}
}

public sealed class IoCDynamic(IManager initial) : BaseIoCDynamic(initial)
{
	public IManager Value => ResolveUntyped();

    internal override Type ManagerType => ResolveUntyped().GetType();
}
