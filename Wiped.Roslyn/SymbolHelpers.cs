using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Wiped.Roslyn;

public static class SymbolHelpers
{
	private static readonly SymbolEqualityComparer SymbolComparer = SymbolEqualityComparer.Default;

	public static bool ImplementsInterface(this ITypeSymbol baseSymbol, INamedTypeSymbol interfaceSymbol)
	{
		if (SymbolComparer.Equals(baseSymbol, interfaceSymbol))
			return false;

		foreach (var iFace in baseSymbol.AllInterfaces)
		{
			if (SymbolComparer.Equals(iFace, interfaceSymbol))
				return true;

			if (SymbolComparer.Equals(iFace.OriginalDefinition, interfaceSymbol?.OriginalDefinition))
				return true;
		}

		return false;
	}

	public static bool DirectlyImplementsInterface(this ITypeSymbol baseSymbol, INamedTypeSymbol interfaceSymbol)
	{
		var interfaces = baseSymbol.Interfaces.OfType<INamedTypeSymbol>();

		return interfaces.Any(i => SymbolComparer.Equals(i.OriginalDefinition, interfaceSymbol?.OriginalDefinition));
	}

	public static INamedTypeSymbol? GetConstructedGenericInterface(this INamedTypeSymbol baseSymbol, INamedTypeSymbol interfaceSymbol)
	{
		foreach (var iFace in baseSymbol.AllInterfaces)
		{
			if (SymbolComparer.Equals(iFace.OriginalDefinition, interfaceSymbol?.OriginalDefinition))
				return iFace;
		}

		return null;
	}

	public static bool HasAttribute(this ISymbol baseSymbol, INamedTypeSymbol attributeSymbol)
	{
		return baseSymbol.GetAttributes().Any(a => SymbolComparer.Equals(a.AttributeClass?.OriginalDefinition, attributeSymbol?.OriginalDefinition));
	}

	public static AttributeData? GetAttribute(this ISymbol baseSymbol, INamedTypeSymbol attributeSymbol)
	{
		foreach (var attribute in baseSymbol.GetAttributes())
		{
			if (SymbolComparer.Equals(attribute.AttributeClass?.OriginalDefinition, attributeSymbol?.OriginalDefinition))
				return attribute;
		}

		return null;
	}

	public static IEnumerable<AttributeData> GetAllAttributes(this ISymbol baseSymbol, INamedTypeSymbol attributeSymbol)
	{
		foreach (var attribute in baseSymbol.GetAttributes())
		{
			if (SymbolComparer.Equals(attribute.AttributeClass?.OriginalDefinition, attributeSymbol?.OriginalDefinition))
				yield return attribute;
		}
	}

	public static IEnumerable<INamedTypeSymbol> GetAllTypes(this INamespaceSymbol namespaceSymbol)
	{
		foreach (var member in namespaceSymbol.GetMembers())
		{
			if (member is INamespaceSymbol ns)
			{
				foreach (var nested in ns.GetAllTypes())
					yield return nested;
			}
			else if (member is INamedTypeSymbol type)
			{
				yield return type;

				foreach (var nested in type.GetNestedTypes())
					yield return nested;
			}
		}
	}

	public static IEnumerable<INamedTypeSymbol> GetNestedTypes(this INamedTypeSymbol type)
	{
		foreach (var nested in type.GetTypeMembers())
		{
			yield return nested;

			foreach (var deeper in nested.GetNestedTypes())
				yield return deeper;
		}
	}

	public static string GetEnumName(this INamedTypeSymbol baseSymbol, object value)
	{
		foreach (var member in baseSymbol.GetMembers().OfType<IFieldSymbol>())
		{
			if (!member.HasConstantValue)
				continue;

			if (member.ConstantValue.Equals(value))
				return member.Name;
		}

		return value.ToString();
	}

	public static Location GetLocation(this ISymbol symbol) => symbol.Locations.FirstOrDefault() ?? Location.None;
}
