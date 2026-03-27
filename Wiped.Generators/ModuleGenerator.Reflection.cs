using Microsoft.CodeAnalysis;
using System.Text;
using Wiped.Roslyn;

namespace Wiped.Generators;

public sealed partial class ModuleGenerator : IIncrementalGenerator
{
	private static void BuildReflectionRegistry(StringBuilder sb, Compilation compilation)
	{
		sb.AppendLine(
			"""
				public void RegisterReflection(ref ITypeRegistry registry)
				{
			"""
		);

		BuildReflectionBaseRegistry(sb, compilation);
		BuildReflectionAttrRegistry(sb, compilation);

		sb.AppendLine(
			"""
				}
			"""
		);

	}

	private static void BuildReflectionBaseRegistry(StringBuilder sb, Compilation compilation)
	{
		if (compilation.GetTypeByMetadataName(ReflectableBaseUsageAttribute) is not { } reflectableBaseAttr)
			return;

		foreach (var type in compilation.Assembly.GlobalNamespace.GetAllTypes())
		{
			if (type.TypeKind is not TypeKind.Class and not TypeKind.Struct)
				continue;

			if (type.IsAbstract)
				continue;

			if (type.IsGenericType || type.TypeParameters.Any()) //no open generics for now
				continue;

			if (!TryGetReflectionVisibility(type, out var visibility))
				continue;

			foreach (var baseType in GetReflectableBases(type, reflectableBaseAttr))
			{
				sb.AppendLine(
					$"""
							registry.RegisterDerived<{baseType.ToDisplayString()}, {type.ToDisplayString()}>({visibility});
					"""
				);
			}
		}

		static IEnumerable<INamedTypeSymbol> GetReflectableBases(INamedTypeSymbol type, INamedTypeSymbol reflectableBaseAttr)
		{
			for (var current = type.BaseType; current != null; current = current.BaseType)
			{
				if (current.GetAttribute(reflectableBaseAttr) is not null)
					yield return current;
			}

			foreach (var iface in type.AllInterfaces)
			{
				if (iface.GetAttribute(reflectableBaseAttr) is not null)
					yield return iface;
			}
		}
	}

	private static void BuildReflectionAttrRegistry(StringBuilder sb, Compilation compilation)
	{
		if (compilation.GetTypeByMetadataName(ReflectableAttributeUsageAttribute) is not { } reflectableAttrAtrr)
			return;

		List<INamedTypeSymbol> reflectableAttributes = [];
		foreach (var type in compilation.GetAllTypes())
		{
			if (!type.IsAttribute(compilation))
				continue;

			if (type.GetAttribute(reflectableAttrAtrr) is not { })
				continue;

			reflectableAttributes.Add(type);
		}

		if (!reflectableAttributes.Any())
			return;

		foreach (var type in compilation.Assembly.GlobalNamespace.GetAllTypes())
		{
			if (!TryGetReflectionVisibility(type, out var visibility))
				continue;

			foreach (var attrType in reflectableAttributes)
			{
				foreach (var attr in type.GetAllAttributes(attrType))
				{
    				var ctorArgs = BuildCtorArgs(attr);

					sb.AppendLine(
						$"""
								registry.RegisterAttribute<{type.ToDisplayString()}, {attrType.ToDisplayString()}>(new {attrType.ToDisplayString()}({ctorArgs}), {visibility});
						"""
					);
				}
			}
		}

		static string BuildCtorArgs(AttributeData attr)
		{
			List<string> args = [];

			foreach (var arg in attr.ConstructorArguments)
				args.Add(arg.FormatConstant());

			foreach (var named in attr.NamedArguments)
				args.Add($"{named.Key} = {named.Value.FormatConstant()}");

			return string.Join(", ", args);
		}
	}

	private static bool TryGetReflectionVisibility(INamedTypeSymbol type, out string visibility)
	{
		switch (type.DeclaredAccessibility)
		{
			case Accessibility.Public:
				visibility = "ReflectionVisibility.Content";
				return true;
			case Accessibility.Internal:
				visibility = "ReflectionVisibility.Engine";
				return true;
			default:
				visibility = string.Empty;
				return false;
		}
	}
}
