using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Wiped.Shared.IoC;

public static partial class IoCManager
{
	private static readonly Dictionary<Type, Type> _bindings = [];
	private static readonly Dictionary<Type, IoCInstance> _instances = [];

	private static readonly Dictionary<Type, Dictionary<Enum, Type>> _switchableBindings = [];
	private static readonly Dictionary<Type, Enum> _currentSelectors = [];
	private static readonly Dictionary<Type, HashSet<Type>> _selectorGroups = [];

	private static readonly Dictionary<Type, List<WeakReference<BaseIoCDynamic>>> _dynamicRefs = [];

	private static IoCLifecycle _state = IoCLifecycle.Constructing;

	private const BindingFlags InjectionFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

	public static bool TryResolve<T>([NotNullWhen(true)] out IoCDynamic<T>? val) where T : IManager
	{
		if (_state == IoCLifecycle.Constructing)
			throw new InvalidOperationException("Resolution not allowed yet");

		var type = typeof(T);
		if (ResolveManager(type, false) is not { } resolved)
		{
			val = null;
			return false;
		}

		val = new IoCDynamic<T>(resolved);
		RegisterDynamic(type, val);
		return true;
	}

	public static bool TryResolve(Type type, [NotNullWhen(true)] out IoCDynamic? val)
	{
		if (_state == IoCLifecycle.Constructing)
			throw new InvalidOperationException("Resolution not allowed yet");

		if (ResolveManager(type, false) is not { } resolved)
		{
			val = null;
			return false;
		}

		val = new IoCDynamic(resolved);
		RegisterDynamic(type, val);
		return true;
	}

	public static IoCDynamic<T> Resolve<T>() where T : IManager
	{
		if (_state == IoCLifecycle.Constructing)
			throw new InvalidOperationException("Resolution not allowed yet");

		var type = typeof(T);
		var resolved = ResolveManager(type, true)!;
		var wrapper = new IoCDynamic<T>(resolved);
		RegisterDynamic(type, wrapper);
		return wrapper;
	}

	public static IoCDynamic Resolve(Type type)
	{
		if (_state == IoCLifecycle.Constructing)
			throw new InvalidOperationException("Resolution not allowed yet");

		var resolved = ResolveManager(type, true)!;
		var wrapper = new IoCDynamic(resolved);
		RegisterDynamic(type, wrapper);
		return wrapper;
	}

	internal static IEnumerable<IManager> GetAllResolved()
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

		static void InjectMember(object target, Type targetType, MemberInfo member, Type dynamicType, Action<object> setFunc)
		{
			if (!typeof(BaseIoCDynamic).IsAssignableFrom(dynamicType))
				throw new InvalidOperationException($"Dependency {member.Name} in {targetType.FullName} must be IoCDynamic<IManager>");

			var managerType = GetIocDynamicType(dynamicType);
			var instance = ResolveManager(managerType, true);
			var wrapper = (BaseIoCDynamic)Activator.CreateInstance(dynamicType, instance)!;
			RegisterDynamic(managerType, wrapper);
			setFunc(wrapper);
		}
	}

	internal static void CreateInstances()
	{
		if (_state != IoCLifecycle.Resolving)
			throw new InvalidOperationException($"Tried to create instances while not resolving");

		foreach (var (_, implType) in _bindings)
			AddInstance(implType);

		foreach (var (_, selectorMap) in _switchableBindings)
		{
			foreach (var implType in selectorMap.Values)
				AddInstance(implType);
		}

		foreach (var instance in _instances.Values)
			ResolveDependencies(instance.Instance);

		foreach (var (interfaceType, map) in _switchableBindings)
		{
			var selector = ExtractDefaultSelector(interfaceType);
			if (!map.ContainsKey(selector))
				throw new InvalidOperationException($"Default selector {selector} not registered for {interfaceType.FullName}");

			_currentSelectors[interfaceType] = selector;
		}

		_state = IoCLifecycle.Frozen;

		static void AddInstance(Type type)
		{
			if (_instances.ContainsKey(type))
				return;

			var instance = (IManager)Activator.CreateInstance(type)! ?? throw new InvalidOperationException($"Failed to create {type.FullName}");
			_instances[type] = new(instance);
		}
	}

	private static IManager? ResolveManager(Type managerType, bool throwIfNotFound = true)
	{
		// normal binding
		if (_bindings.TryGetValue(managerType, out var implType))
		{
			if (!_instances.TryGetValue(implType, out var existing))
				throw new InvalidOperationException($"{managerType.FullName} has an implmenentation of {implType.FullName} yet has no instance");

			return existing.Instance;
		}

		if (_switchableBindings.TryGetValue(managerType, out var map))
		{
			var selector = _currentSelectors[managerType];
			var selectedImpl = map[selector];

			if (!_instances.TryGetValue(selectedImpl, out var existing))
				throw new InvalidOperationException($"{managerType.FullName} has an implmenentation of {selectedImpl.FullName} yet has no instance");

			return existing.Instance;
		}

		return !throwIfNotFound ? null
			: throw new InvalidOperationException($"Cannot find resolve type {managerType.FullName}");
	}

	private static void RegisterDynamic(Type managerType, BaseIoCDynamic dynamic)
	{
		if (!_dynamicRefs.TryGetValue(managerType, out var list))
			_dynamicRefs[managerType] = list = [];

		list.Add(new(dynamic));
	}

	public static void Bind<TInterface, TImplementation>()
		where TInterface : IManager
		where TImplementation : TInterface
	{
		if (_state != IoCLifecycle.Constructing)
			throw new InvalidOperationException($"Tried binding when Ioc isnt constructable");

		var interfaceType = typeof(TInterface);
		var implType = typeof(TImplementation);

		var isSwitchable = false;
		foreach (var ifaceAttr in GetSelectableAttributes(interfaceType))
		{
			isSwitchable = true;

			var selector = ExtractSelectorFromImplementation(implType, interfaceType);
			var selectorType = selector.GetType();

			if (!_selectorGroups.TryGetValue(selectorType, out var group))
				_selectorGroups[selectorType] = group = [];

			group.Add(interfaceType);

			if (!_switchableBindings.TryGetValue(interfaceType, out var map))
				_switchableBindings[interfaceType] = map = [];

			if (!map.TryAdd(selector, implType))
				throw new InvalidOperationException($"Duplicate selector {selector} for {interfaceType.FullName}");
		}

		if (!isSwitchable)
		{
			if (!interfaceType.IsInterface)
				throw new InvalidOperationException($"{interfaceType.FullName} must be an interface");

			if (_bindings.ContainsKey(interfaceType))
				throw new InvalidOperationException($"Duplicate binding for {interfaceType.FullName}");

			_bindings[interfaceType] = implType;
		}
	}

	public static void Switch<TSelector>(TSelector selector) where TSelector : Enum
	{
		if (_state != IoCLifecycle.Frozen)
			throw new InvalidOperationException("Switching only allowed after freeze");

		var selectorType = typeof(TSelector);

		if (!_selectorGroups.TryGetValue(selectorType, out var interfaces))
        	throw new InvalidOperationException($"No switchable managers registered for {selectorType.Name}");

		foreach (var interfaceType in interfaces)
		{
			if (!_switchableBindings.TryGetValue(interfaceType, out var map))
				throw new InvalidOperationException($"{interfaceType.Name} is not switchable");

			if (!map.TryGetValue(selector, out var implType))
				throw new InvalidOperationException($"Selector {selector} not registered");

			if (!_instances.TryGetValue(implType, out var nextInstance))
				throw new InvalidOperationException($"{interfaceType.FullName} has an implmenentation of {implType.FullName} yet has no instance");

			// update any ioc containers underlying implementation
			if (_dynamicRefs.TryGetValue(interfaceType, out var list))
			{
				for (var i = list.Count - 1; i >= 0; i--) // walk the list backwards otherwise later entries index messes up
				{
					var weak = list[i];
					if (!weak.TryGetTarget(out var dynamic)) // remove stale references
					{
						list.RemoveAt(i);
						continue;
					}

					dynamic.Swap(nextInstance.Instance);
				}
			}

			_currentSelectors[interfaceType] = selector;
		}
	}

	internal static void AllowResolving()
	{
		if (_state != IoCLifecycle.Constructing)
			throw new InvalidOperationException($"Tried to allow resolving yet IoC isn't contructable");

		_state = IoCLifecycle.Resolving;
	}

	private enum IoCLifecycle : byte
	{
		Constructing,   // bindings allowed, no resolution
		Resolving,      // resolution + injection allowed
		Frozen          // fully locked, runtime-only
	}
}
