using System.Reflection;

namespace Wiped.Shared.IoC;

public static partial class IoCManager
{
	private const BindingFlags FieldExtractionFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

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

	private static Enum ExtractDefaultSelector(Type interfaceType)
	{
		var attrs = interfaceType.GetCustomAttributes(false);

		foreach (var attr in attrs)
		{
			var attrType = attr.GetType();

			if (!attrType.IsGenericType)
				continue;

			if (attrType.GetGenericTypeDefinition() != typeof(SwitchableManagerAttribute<>))
				continue;

			var field = attrType.GetField(nameof(SwitchableManagerAttribute<>.Default), FieldExtractionFlags)
				?? throw new InvalidOperationException($"Default selector missing on {attrType}");

			return (Enum)field.GetValue(attr)!;
		}

		throw new InvalidOperationException($"{interfaceType.FullName} is switchable but has no default selector");
	}

	private static IEnumerable<Attribute> GetSelectableAttributes(Type interfaceType)
	{
		var attrs = interfaceType.GetCustomAttributes(false);
		foreach (var attr in attrs)
		{
			var attrType = attr.GetType();
			if (!attrType.IsGenericType)
				continue;

			if (attrType.GetGenericTypeDefinition() != typeof(SwitchableManagerAttribute<>))
				continue;

			yield return (Attribute)attr;
		}
	}

	private static Enum ExtractSelectorFromImplementation(Type implType, Type interfaceType)
	{
		var attrs = implType.GetCustomAttributes(false);

		foreach (var attr in attrs)
		{
			var attrType = attr.GetType();

			if (!attrType.IsGenericType)
				continue;

			if (attrType.GetGenericTypeDefinition() != typeof(SwitchableManagerSelectorAttribute<,>))
				continue;

			var args = attrType.GetGenericArguments();
			if (args[0] != interfaceType) // make sure its not some other binding
				continue;

			var field = attrType.GetField(nameof(SwitchableManagerSelectorAttribute<,>.Selector), FieldExtractionFlags)
					?? throw new InvalidOperationException($"Selector field missing on {attrType}");

			return (Enum)field.GetValue(attr)!;
		}

		throw new InvalidOperationException($"{implType.FullName} missing ManagerSelector attribute");
	}
}
