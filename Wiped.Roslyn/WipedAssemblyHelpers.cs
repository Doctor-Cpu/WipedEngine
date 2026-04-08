using Microsoft.CodeAnalysis;

namespace Wiped.Roslyn;

public static class WipedAssemblyHelpers
{
	public static bool IsWipedAssembly(this IAssemblySymbol assembly, Compilation compilation, INamedTypeSymbol? engineAssemblySymbol = null, INamedTypeSymbol? contentAssemblySymbol = null)
	{
        engineAssemblySymbol ??= compilation.GetTypeByMetadataName(EngineAssemblyAttribute)
        	?? throw new InvalidOperationException("Cannot find EngineAssemblyAttribute type");

        contentAssemblySymbol ??= compilation.GetTypeByMetadataName(ContentAssemblyAttribute)
        	?? throw new InvalidOperationException("Cannot find ContentAssemblyAttribute type");

		return assembly.GetAttributes(engineAssemblySymbol, contentAssemblySymbol).Any();
	}

	public static string GetWipedRootNamespace(this IAssemblySymbol assembly, Compilation compilation, INamedTypeSymbol? engineAssemblySymbol = null, INamedTypeSymbol? contentAssemblySymbol = null)
	{
		var contentAssembly = GetWipedAssemblyAttribute(assembly, compilation, contentAssemblySymbol);
		var arg = contentAssembly.ConstructorArguments[0];

		if (arg.Value is string rootNamespace && !string.IsNullOrWhiteSpace(rootNamespace))
			return rootNamespace;

		throw new InvalidOperationException($"{assembly.Name} does not have a valid rootnamespace");
	}

	public static string GetCurrentAssemblyWipedRootNamespace(this Compilation compilation, INamedTypeSymbol? engineAssemblySymbol = null, INamedTypeSymbol? contentAssemblySymbol = null)
		=> GetWipedRootNamespace(compilation.Assembly, compilation, engineAssemblySymbol, contentAssemblySymbol);

	public static IEnumerable<IAssemblySymbol> GetWipedAssemblies(this Compilation compilation, INamedTypeSymbol? engineAssemblySymbol = null, INamedTypeSymbol? contentAssemblySymbol = null)
	{
        engineAssemblySymbol ??= compilation.GetTypeByMetadataName(EngineAssemblyAttribute)
        	?? throw new InvalidOperationException("Cannot find EngineAssemblyAttribute type");

        contentAssemblySymbol ??= compilation.GetTypeByMetadataName(ContentAssemblyAttribute)
        	?? throw new InvalidOperationException("Cannot find ContentAssemblyAttribute type");

		foreach (var assembly in compilation.SourceModule.ReferencedAssemblySymbols)
		{
			// skip self
			if (SymbolEqualityComparer.Default.Equals(assembly, compilation.Assembly))
				continue;

			if (assembly.HasAnyAttributes(engineAssemblySymbol, contentAssemblySymbol))
				yield return assembly;
		}
	}

	private static AttributeData GetWipedAssemblyAttribute(IAssemblySymbol assembly, Compilation compilation, INamedTypeSymbol? engineAssemblySymbol = null, INamedTypeSymbol? contentAssemblySymbol = null)
	{
        engineAssemblySymbol ??= compilation.GetTypeByMetadataName(EngineAssemblyAttribute)
        	?? throw new InvalidOperationException("Cannot find EngineAssemblyAttribute type");

        contentAssemblySymbol ??= compilation.GetTypeByMetadataName(ContentAssemblyAttribute)
        	?? throw new InvalidOperationException("Cannot find ContentAssemblyAttribute type");

		var attrs = assembly.GetAttributes(engineAssemblySymbol, contentAssemblySymbol).ToArray();
		switch (attrs.Length)
		{
			case < 1:
				throw new InvalidOperationException($"Found multiple WipedAssembly attributes on {assembly.Name} assembly");
			case 1:
				return attrs[0];
			case > 1:
				throw new InvalidOperationException($"Assembly {assembly.Name} is not a WipedAssembly");
		}
	}
}
