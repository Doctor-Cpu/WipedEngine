using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Wiped.Shared.IoC;

public sealed class IoCContainer
{
	private readonly Dictionary<Type, Type> _bindings = [];
	private readonly Dictionary<Type, object> _bindingInstances = [];

	private bool _frozen;

	private const BindingFlags InjectionFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

	public void Bind<TInterface, TImpl>() where TImpl : BaseManager, TInterface, new()
	{
		if (_frozen)
			throw new InvalidOperationException("IoC is frozen");

		_bindings[typeof(TInterface)] = typeof(TImpl);
	}

	public void Import(IoCContainer other)
	{
		if (_frozen)
			throw new InvalidOperationException("IoC is frozen");

		foreach (var (inter, impl) in other._bindings)
		{
			if (_bindings.ContainsKey(inter))
				throw new InvalidOperationException($"Binding conflict for {inter.FullName}");

			_bindings[inter] = impl;
		}
	}

	public bool TryResolve<T>([NotNullWhen(true)] out T? val)
	{
		if (!_frozen)
			throw new InvalidOperationException("IoC is not frozen");

		if (TryResolve(typeof(T), out var temp))
		{
			val = (T)temp;
			return true;
		}
		else
		{
			val = default;
			return false;
		}
	}

	public void ResolveDependencies(object instance, bool throwFrozen = true)
	{
		if (!_frozen)
		{
			if (throwFrozen)
				throw new InvalidOperationException("IoC is not frozen");
			else
				return;
		}

		var type = instance.GetType();

		foreach (var field in type.GetFields(InjectionFlags))
		{
			if (!field.IsDefined(typeof(DependencyAttribute)))
				continue;

			if (TryResolve(field.FieldType, out var dependency))
				field.SetValue(instance, dependency);
		}

		foreach (var prop in type.GetProperties(InjectionFlags))
		{
			if (!prop.IsDefined(typeof(DependencyAttribute)) || !prop.CanWrite)
				continue;

			if (TryResolve(prop.PropertyType, out var dependency))
				prop.SetValue(instance, dependency);
		}
	}

	private bool TryResolve(Type type, [NotNullWhen(true)] out object? val)
	{
		if (!_frozen)
			throw new InvalidOperationException("IoC is not frozen");

		if (_bindingInstances.TryGetValue(type, out var existing))
		{
			val = existing;
			return true;
		}

		if (!_bindings.TryGetValue(type, out var implType))
		{
			val = null;
			return false;
		}

		var instance = Activator.CreateInstance(implType) ?? throw new InvalidOperationException($"Failed to create {implType.FullName}");

		_bindingInstances[type] = instance;

		val = instance;
		return true;
	}

	public void Freeze()
	{
		_frozen = true;
	}

	internal void Initialize()
	{
		HashSet<Type> visited = new(_bindings.Count);
		foreach (var type in _bindings.Values)
		{
			if (!visited.Add(type))
				continue;

			if (!TryResolve(type, out var val))
				throw new InvalidOperationException($"Tried to fetch {type.FullName} when initializing");

			var manager = (BaseManager)val;
			manager.Initialize();
		}
	}

	internal void Shutdown()
	{
		HashSet<Type> visited = new(_bindings.Count);
		foreach (var type in _bindings.Values)
		{
			if (!visited.Add(type))
				continue;

			if (!TryResolve(type, out var val))
				throw new InvalidOperationException($"Tried to fetch {type.FullName} when shutting down");

			var manager = (BaseManager)val;
			manager.Shutdown();
		}
	}

	internal IoCContainer()
	{
	}
}
