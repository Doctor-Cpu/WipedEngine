using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Wiped.Shared.IoC;
using Wiped.Shared.VFS;

namespace Wiped.Shared.Prototypes;

[AutoBind(typeof(IPrototypeManager), typeof(IEnginePrototypeManager))]
internal sealed class PrototypeManager : IPrototypeManager, IEnginePrototypeManager, IHotReloadable
{
	private FrozenDictionary<Type, Dictionary<string, IPrototype>> _prototypes = default!;

	private const BindingFlags InheritanceFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

	public void Initialize()
	{
		Dictionary<Type, Dictionary<string, IPrototype>> tempProtoDict = [];
		_prototypes = tempProtoDict.ToFrozenDictionary();
	}

	public bool TryGet<T>(ProtoId<T> id, [NotNullWhen(true)] out T? prototype) where T: IPrototype
	{
		if (!TryGetWithAbstracts(id, out prototype))
			return false;

		return !prototype.Abstract;
	}

	public bool TryGetWithAbstracts<T>(ProtoId<T> id, [NotNullWhen(true)] out T? prototype) where T: IPrototype
	{
		if (!_prototypes[typeof(T)].TryGetValue(id.Id, out var tempProto))
		{
			prototype = default;
			return false;
		}

		prototype = (T)tempProto;
		return true;
	}

	public IEnumerable<T> GetAllOfType<T>() where T: IPrototype
	{
		foreach (var prototype in _prototypes[typeof(T)].Values)
			yield return (T)prototype;
	}

	public void Register(IEnumerable<IPrototype> prototypes)
	{
		Dictionary<Type, Dictionary<string, IPrototype>> protoDict = _prototypes.ToDictionary();

		foreach (var prototype in prototypes)
		{
			var type = prototype.GetType();

			if (!_prototypes.TryGetValue(type, out var dict))
			{
				dict = new(StringComparer.OrdinalIgnoreCase);
				protoDict[type] = dict;
			}

			dict[prototype.Id] = prototype;
		}

		_prototypes = protoDict.ToFrozenDictionary();
	}

	public void ResolveInheritance()
	{
		foreach (var typeDict in _prototypes.Values)
		{
			foreach (var prototype in typeDict.Values)
				ApplyInheritance(prototype);
		}
	}

	private void ApplyInheritance(IPrototype prototype, HashSet<string>? visited = null)
	{
		visited ??= new(StringComparer.OrdinalIgnoreCase);
		if (!visited.Add(prototype.Id))
			throw new InvalidOperationException($"Circular prototype inheritance detected at {prototype.Id}");

		var type = prototype.GetType();

		foreach (var parentId in prototype.Parents)
		{
			if (!TryGetWithAbstracts(parentId, out var parent))
            	throw new InvalidOperationException($"Parent prototype {parentId} not found for {prototype.Id}");

			ApplyInheritance(parent, visited);

			foreach (var field in type.GetFields(InheritanceFlags))
			{
				// skip readonly fields
				if (field.IsInitOnly)
					continue;

				if (field.IsDefined(typeof(NeverInheritAttribute)))
					continue;

				var alwaysInherit = field.IsDefined(typeof(AlwaysInheritAttribute));

				var childValue = field.GetValue(prototype);
				var defaultValue = GetDefault(field.FieldType);

				if (alwaysInherit || Equals(childValue, defaultValue))
					field.SetValue(prototype, field.GetValue(parent));
			}

			foreach (var prop in type.GetProperties(InheritanceFlags))
			{
				if (!prop.CanWrite)
					continue;

				if (prop.IsDefined(typeof(NeverInheritAttribute)))
					continue;

				var alwaysInherit = prop.IsDefined(typeof(AlwaysInheritAttribute));

				var childValue = prop.GetValue(prototype);
				var defaultValue = GetDefault(prop.PropertyType);

				if (alwaysInherit || Equals(childValue, defaultValue))
					prop.SetValue(prototype, prop.GetValue(parent));
			}

			static object? GetDefault(Type t) => t.IsValueType ? Activator.CreateInstance(t) : null;
		}

		visited.Remove(prototype.Id);
	}
}
