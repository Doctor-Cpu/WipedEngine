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

		if (compilation.GetTypeByMetadataName(ReflectableAttribute) is not { } reflectableAttr)
			goto end;

		foreach (var type in compilation.Assembly.GlobalNamespace.GetAllTypes())
		{
			if (type.TypeKind is not TypeKind.Class and not TypeKind.Struct)
				continue;

			if (type.IsAbstract)
				continue;

			if (type.IsGenericType || type.TypeParameters.Any()) //no open generics for now
				continue;

			if (type.DeclaredAccessibility switch
				{
					Accessibility.Public => "ReflectionVisibility.Content",
					Accessibility.Internal => "ReflectionVisibility.Engine",
					_ => null
				}
				// code smell alert
				// c# doesnt provide a way to nicely continue within a switch so this it is
				is not { } visibility)
			{
				continue;
			}

			foreach (var baseType in GetReflectableBases(type, reflectableAttr))
			{
				sb.AppendLine(
					$"""
							registry.RegisterDerived<{baseType.ToDisplayString()}, {type.ToDisplayString()}>({visibility});
					"""
				);
			}
		}

		end:

		sb.AppendLine(
			"""
				}
			"""
		);
	}

	private static IEnumerable<INamedTypeSymbol> GetReflectableBases(INamedTypeSymbol type, INamedTypeSymbol reflectableAttr)
	{
		for (var current = type.BaseType; current != null; current = current.BaseType)
		{
			if (current.GetAttribute(reflectableAttr) is not null)
				yield return current;
		}

		foreach (var iface in type.AllInterfaces)
		{
			if (iface.GetAttribute(reflectableAttr) is not null)
				yield return iface;
		}
	}
}
