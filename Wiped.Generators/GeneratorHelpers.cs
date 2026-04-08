using Microsoft.CodeAnalysis;
using Wiped.Roslyn;

namespace Wiped.Generators;

public static class GeneratorHelpers
{
	public const string NamespaceSuffix = "Generated";

	public static string GetGeneratorNamespace(this IAssemblySymbol assembly, Compilation compilation, INamedTypeSymbol? engineAssemblySymbol = null, INamedTypeSymbol? contentAssemblySymbol = null)
	{
		return $"{assembly.GetWipedRootNamespace(compilation, engineAssemblySymbol, contentAssemblySymbol)}.{NamespaceSuffix}";
	}
}
