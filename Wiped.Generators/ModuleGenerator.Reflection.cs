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

		var allTypes = compilation.Assembly.GlobalNamespace.GetAllTypes().ToList();
		List<INamedTypeSymbol> reflectableBases = new(allTypes.Count);
		foreach (var type in allTypes)
		{
			if (type.GetAttribute(reflectableAttr) is { })
				reflectableBases.Add(type);
		}

		// nothing to do so save the expensive checks ahead
		if (!reflectableBases.Any())
			goto end;

		foreach (var type in allTypes)
		{
			if (type.TypeKind is not TypeKind.Class and not TypeKind.Struct)
				continue;

			if (type.IsAbstract)
				continue;

			if (type.IsUnboundGenericType || type.TypeParameters.Any()) //no open generics for now
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

			foreach (var baseType in reflectableBases)
			{
				if (!type.InheritsFrom(baseType) && !type.ImplementsInterface(baseType))
					continue;

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

}
