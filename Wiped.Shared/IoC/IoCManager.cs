using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Wiped.Shared.IoC;

public static class IoCManager
{
	private static readonly Dictionary<Type, Type> _bindings = [];
	private static readonly Dictionary<Type, Dictionary<object, Type>> _switchableBindings = [];
	private static readonly Dictionary<Type, IoCInstance> _instances = [];

	private static IoCLifecycle _state = IoCLifecycle.Constructing;

	private const BindingFlags InjectionFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

	public static bool TryResolve<T>([NotNullWhen(true)] out IoCDynamic<T>? val) where T : IManager
	{
		if (_state == IoCLifecycle.Constructing)
			throw new InvalidOperationException("Resolution not allowed yet");

		var type = typeof(T);
		if (!_bindings.TryGetValue(type, out var implType))
		{
			val = null;
			return false;
		}

		if (_instances.TryGetValue(implType, out var existing))
		{
			val = new(existing.Instance);
			return true;
		}

		throw new InvalidOperationException($"{type.FullName} has an implmenentation of {implType.FullName} yet has no instance");
	}

	public static bool TryResolve(Type type, [NotNullWhen(true)] out IoCDynamic? val)
	{
		if (_state == IoCLifecycle.Constructing)
			throw new InvalidOperationException("Resolution not allowed yet");

		if (!_bindings.TryGetValue(type, out var implType))
		{
			val = null;
			return false;
		}

		if (_instances.TryGetValue(implType, out var existing))
		{
			val = new(existing.Instance);
			return true;
		}

		throw new InvalidOperationException($"{type.FullName} has an implmenentation of {implType.FullName} yet has no instance");
	}

	public static IoCDynamic<T> Resolve<T>() where T : IManager
	{
		if (_state == IoCLifecycle.Constructing)
			throw new InvalidOperationException("Resolution not allowed yet");

		var type = typeof(T);
		return new(ResolveManager(type));
	}

	public static IoCDynamic Resolve(Type type)
	{
		if (_state == IoCLifecycle.Constructing)
			throw new InvalidOperationException("Resolution not allowed yet");

		return new(ResolveManager(type));
	}

	internal static IEnumerable<object> GetAllResolved()
	{
		foreach (var holder in _instances.Values)
			yield return holder.Instance;
	}

	public static void ResolveDependencies(object instance)
	{
		if (_state == IoCLifecycle.Constructing)
			return;

		var type = instance.GetType();

		foreach (var field in type.GetFields(InjectionFlags))
		{
			if (!field.IsDefined(typeof(DependencyAttribute)))
				continue;

			InjectMember(instance, type, field, field.FieldType, wrapper => field.SetValue(instance, wrapper));
		}

		foreach (var prop in type.GetProperties(InjectionFlags))
		{
			if (!prop.IsDefined(typeof(DependencyAttribute)) || !prop.CanWrite)
				continue;

			InjectMember(instance, type, prop, prop.PropertyType, wrapper => prop.SetValue(instance, wrapper));
		}
	}

	internal static void CreateInstances()
	{
		if (_state != IoCLifecycle.Resolving)
			throw new InvalidOperationException($"Tried to create instances while not resolving");

		foreach (var (type, implType) in _bindings)
		{
			var instance = (IManager)Activator.CreateInstance(implType)! ?? throw new InvalidOperationException($"Failed to create {implType.FullName}");
			_instances[implType] = new(instance);
		}

		foreach (var instance in _instances.Values)
			ResolveDependencies(instance.Instance);

		_state = IoCLifecycle.Frozen;
	}

	private static IManager ResolveManager(Type managerType)
	{
		if (!_bindings.TryGetValue(managerType, out var implType))
			throw new InvalidOperationException($"Cannot find resolve type {managerType.FullName}");

		if (!_instances.TryGetValue(implType, out var existing))
			throw new InvalidOperationException($"{managerType.FullName} has an implmenentation of {implType.FullName} yet has no instance");

		return existing.Instance;
	}

	private static void InjectMember(object target, Type targetType, MemberInfo member, Type dynamicType, Action<object> setFunc)
	{
		if (!typeof(BaseIoCDynamic).IsAssignableFrom(dynamicType))
			throw new InvalidOperationException($"Dependency {member.Name} in {targetType.FullName} must be IoCDynamic<IManager>");

		var managerType = GetIocDynamicType(dynamicType);
		var instance = ResolveManager(managerType);
    	var wrapper = Activator.CreateInstance(dynamicType, instance)!;
		setFunc(wrapper);
	}

	private static Type GetIocDynamicType(Type dynamicType) // incase any further wrappers (x to doubt)
	{
		if (dynamicType.IsGenericType)
		{
			if (dynamicType.GetGenericTypeDefinition() == typeof(IoCDynamic<>))
				return dynamicType.GetGenericArguments()[0];
		}
		else if (dynamicType == typeof(IoCDynamic))
		{
			throw new NotImplementedException();
		}

		throw new InvalidOperationException($"Cannot determine manager type for {dynamicType}");
	}

	public static void Bind<TInterface, TImplementation>()
		where TInterface : IManager
		where TImplementation : TInterface
	{
		if (_state != IoCLifecycle.Constructing)
			throw new InvalidOperationException($"Tried binding when Ioc isnt constructable");

		var interfaceType = typeof(TInterface);
		var implType = typeof(TImplementation);

		if (!interfaceType.IsInterface)
			throw new InvalidOperationException($"{interfaceType.FullName} must be an interface");

		if (_bindings.ContainsKey(interfaceType))
			throw new InvalidOperationException($"Duplicate binding for {interfaceType.FullName}");

		_bindings[interfaceType] = implType;
	}

	/*
	public static void BindSwitchable<TInterface, TImplementation, TSelector>()
		where TSelector : Enum
		where TInterface : ISwitchableManager<TSelector>
		where TImplementation : TInterface
	{
		if (_state != IoCLifecycle.Constructing)
			throw new InvalidOperationException("Tried binding when IoC isnt constructable");

		var interfaceType = typeof(TInterface);
		var implType = typeof(TImplementation);
		var selectorType = typeof(TSelector);

		if (!interfaceType.IsInterface)
			throw new InvalidOperationException($"{interfaceType.FullName} must be an interface");

		if (!_switchableBindings.TryGetValue(interfaceType, out var map))
			_switchableBindings[interfaceType] = map = [];

		if (map.ContainsKey(selectorType))
			throw new InvalidOperationException($"Duplicate selector {selectorType} for {interfaceType.FullName}");

		map[selectorType] = implType;
	}

	public static void Switch<TSwitchable, TSelector>(TSelector selector)
		where TSelector : Enum
		where TSwitchable : ISwitchableManager<TSelector>
	{
	}
	*/

	internal static void AllowResolving()
	{
		if (_state != IoCLifecycle.Constructing)
			throw new InvalidOperationException($"Tried to allow resolving yet IoC isn't contructable");

		// TODO: specify a default implementation so this works
		/*
		foreach (var (interfaceType, map) in _switchableBindings)
		{
			foreach (var (selectorType, implType) in map)
			{
				_bindings[interfaceType] = implType;
			}
		}
		*/

		_state = IoCLifecycle.Resolving;
	}

	private enum IoCLifecycle : byte
	{
		Constructing,   // bindings allowed, no resolution
		Resolving,      // resolution + injection allowed
		Frozen          // fully locked, runtime-only
	}
}
