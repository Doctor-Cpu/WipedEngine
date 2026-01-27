using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Wiped.Shared.IoC;

public sealed class IoCContainer
{
	private readonly Dictionary<Type, Type> _bindings = [];
	private readonly Dictionary<Type, object> _bindingInstances = [];

	private IoCLifecycle _state = IoCLifecycle.Constructing;

	private const BindingFlags InjectionFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

	private void Bind(Type interfaceType, Type implType)
	{
		if (_state != IoCLifecycle.Constructing)
			throw new InvalidOperationException("Bindings are frozen");

		if (!interfaceType.IsAssignableFrom(implType))
			throw new InvalidOperationException($"{implType.FullName} does not implement {interfaceType.FullName}");

		if (!typeof(IManager).IsAssignableFrom(implType))
			throw new InvalidOperationException($"{implType.FullName} must implement IManager");

		_bindings[interfaceType] = implType;
	}

	public bool TryResolve<T>([NotNullWhen(true)] out T? val)
	{
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

	public bool TryResolve(Type type, [NotNullWhen(true)] out object? val)
	{
		if (_state == IoCLifecycle.Constructing)
			throw new InvalidOperationException("Resolution not allowed yet");

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

		if (_state == IoCLifecycle.Frozen)
			throw new InvalidOperationException($"Type {type.FullName} was not resolved before freeze");


		var instance = Activator.CreateInstance(implType) ?? throw new InvalidOperationException($"Failed to create {implType.FullName}");

		_bindingInstances[type] = instance;

		IoCManager.ResolveDependencies(instance);

		val = instance;
		return true;
	}


	public void InjectInto(object instance)
	{
		if (_state == IoCLifecycle.Constructing)
			return;

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

	internal void TransitionTo(IoCLifecycle next)
	{
		if (next < _state)
			throw new InvalidOperationException($"Invalid IoC transition {_state} -> {next}");

		_state = next;
	}

	internal void AutoBind()
	{
		foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
		{
			foreach (var type in asm.GetTypes())
			{
				if (type.IsAbstract || type.IsInterface)
					continue;

				var attrs = type.GetCustomAttributes<AutoBindAttribute>();
				foreach (var attr in attrs)
				{
					foreach (var serviceType in attr.ServiceTypes)
						Bind(serviceType, type);
				}
			}
		}
	}

	internal IoCContainer()
	{
	}
}
