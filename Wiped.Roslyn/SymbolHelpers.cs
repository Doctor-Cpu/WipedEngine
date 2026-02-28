using Microsoft.CodeAnalysis;

namespace Wiped.Roslyn;

public static class SymbolHelpers
{
	private static readonly SymbolEqualityComparer SymbolComparer = SymbolEqualityComparer.Default;

	public static bool ImplementsInterface(this ITypeSymbol baseSymbol, INamedTypeSymbol interfaceSymbol)
	{
		return baseSymbol.AllInterfaces.Any(i => SymbolComparer.Equals(i, interfaceSymbol));
	}

	public static bool HasAttribute(this ISymbol baseSymbol, INamedTypeSymbol attributeSymbol)
	{
		return baseSymbol.GetAttributes().Any(a => SymbolComparer.Equals(a.AttributeClass, attributeSymbol));
	}

	public static AttributeData? GetAttribute(this ISymbol baseSymbol, INamedTypeSymbol attributeSymbol)
	{
		foreach (var attribute in baseSymbol.GetAttributes())
		{
			if (SymbolComparer.Equals(attribute.AttributeClass, attributeSymbol))
				return attribute;
		}

		return null;
	}
}
