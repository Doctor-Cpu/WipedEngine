using Wiped.Shared.IoC;
using Wiped.Shared.Lifecycle;
using Wiped.Shared.Reflection;
using Wiped.Shared.VFS;

namespace Wiped.Shared.CVars;

[AutoBind(typeof(ICVarManager))]
internal sealed class CVarManager : ICVarManager, IHotReloadable
{
	[Dependency] private readonly ILifecycleManager _lifecycle = default!;
	[Dependency] private readonly IReflectionManager _reflection = default!;
	[Dependency] private readonly IEngineReflectionManager _engineReflection = default!;

	public Type[] After => [typeof(ILifecycleManager)];

	private readonly Dictionary<string, ICVar> _byId = [];
	private readonly Dictionary<ICVar, object> _values = [];

	public void Initialize()
	{
		foreach (var module in _lifecycle.GetAll<BaseCVarModule>())
		{
			var moduleType = module.GetType();
			foreach (var cVarInfo in _reflection.GetMemberAttributes<CVarDefAttribute>(moduleType))
			{
				if (_engineReflection.TryGetStaticValue<ICVar>(cVarInfo.Member, out var cVar))
					Register(cVar);
				else
					throw new InvalidOperationException($"Found CVar definition {cVarInfo.Name} in {moduleType} yet could not fetch its value");
			}
		}
	}

	public void Shutdown()
	{
		_byId.Clear();
		_values.Clear();
	}

    private void Register(ICVar cVar)
    {
        if (_byId.ContainsKey(cVar.Id))
            throw new InvalidOperationException($"CVar '{cVar.Id}' already registered.");

        _byId[cVar.Id] = cVar;
		_values[cVar] = cVar.DefaultUntypedValue;
    }

	public T GetValue<T>(CVar<T> cVar) where T : notnull
	{
#if DEBUG
		if (!_values.TryGetValue(cVar, out var value))
			throw new InvalidOperationException($"CVar {cVar} is not registered");

		if (cVar.ValueType != typeof(T))
			throw new InvalidOperationException($"Type mismatch of CVar {cVar}. Expected {typeof(T)} but got {cVar.ValueType}");

		return (T)value;
#else
		return (T)_values[cVar.Id];
#endif
	}

	public void SetValue<T>(CVar<T> cVar, T value) where T : notnull
	{
#if DEBUG
		if (!_values.ContainsKey(cVar))
			throw new InvalidOperationException($"Tried to set value of CVar {cVar} which hasnt been registered");

		if (cVar.ValueType != typeof(T))
			throw new InvalidOperationException($"Type mismatch of CVar {cVar}. Expected {typeof(T)} but got {cVar.ValueType}");
#endif

		_values[cVar] = value;
	}
}
