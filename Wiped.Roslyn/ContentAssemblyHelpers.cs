using Microsoft.CodeAnalysis;

namespace Wiped.Roslyn;

public static class ContentAssemblyHelpers
{
	public static string GetContentRootNamespace(this IAssemblySymbol assembly, Compilation compilation, INamedTypeSymbol? contentAssemblySymbol = null)
	{
		var contentAssembly = GetContentAssemblyAttribute(assembly, compilation, contentAssemblySymbol);
		var arg = contentAssembly.ConstructorArguments[0];

		if (arg.Value is string rootNamespace && !string.IsNullOrWhiteSpace(rootNamespace))
			return rootNamespace;

		throw new InvalidOperationException($"{assembly.Name} does not have a valid rootnamespace");
	}

	public static string GetCurrentAssemblyContentRootNamespace(this Compilation compilation, INamedTypeSymbol? contentAssemblySymbol = null) => GetContentRootNamespace(compilation.Assembly, compilation, contentAssemblySymbol);

	public static IEnumerable<IAssemblySymbol> GetContentAssemblies(this Compilation compilation, INamedTypeSymbol? contentAssemblySymbol = null)
	{
        contentAssemblySymbol ??= compilation.GetTypeByMetadataName(ContentAssemblyAttribute)
        	?? throw new InvalidOperationException("Cannot find ContentAssemblyAttribute type");

		foreach (var assembly in compilation.SourceModule.ReferencedAssemblySymbols)
		{
			// skip self
			if (SymbolEqualityComparer.Default.Equals(assembly, compilation.Assembly))
				continue;

			if (assembly.HasAttribute(contentAssemblySymbol))
				yield return assembly;
		}
	}

	private static AttributeData GetContentAssemblyAttribute(IAssemblySymbol assembly, Compilation compilation, INamedTypeSymbol? contentAssemblySymbol = null)
	{
        contentAssemblySymbol ??= compilation.GetTypeByMetadataName(ContentAssemblyAttribute)
        	?? throw new InvalidOperationException("Cannot find ContentAssemblyAttribute type");

		return assembly.GetAttribute(contentAssemblySymbol)
			?? throw new InvalidOperationException($"{assembly.Name} is does not have {ContentAssemblyAttribute}");
	}
}
