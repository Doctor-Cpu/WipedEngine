using Microsoft.CodeAnalysis;
using Wiped.Roslyn;

namespace Wiped.Generators;

public static class GeneratorHelpers
{
	public const string NamespaceSuffix = "Generated";

	public static string GetGeneratorNamespace(this IAssemblySymbol assembly, Compilation compilation, INamedTypeSymbol? contentAssemblySymbol = null)
	{
		return $"{assembly.GetContentRootNamespace(compilation, contentAssemblySymbol)}.{NamespaceSuffix}";
	}
}
