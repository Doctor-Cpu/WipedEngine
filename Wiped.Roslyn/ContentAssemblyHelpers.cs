using Microsoft.CodeAnalysis;

namespace Wiped.Roslyn;

public static class ContentAssemblyHelpers
{
	public static string GetContentRootNamespace(this IAssemblySymbol assembly, Compilation compilation)
	{
		var contentAssembly = GetContentAssemblyAttribute(assembly, compilation);
		var arg = contentAssembly.ConstructorArguments[0];

		if (arg.Value is string rootNamespace && !string.IsNullOrWhiteSpace(rootNamespace))
			return rootNamespace;

		throw new InvalidOperationException($"{assembly.Name} does not have a valid rootnamespace");
	}

	public static string GetCurrentAssemblyContentRootNamespace(this Compilation compilation) => GetContentRootNamespace(compilation.Assembly, compilation);

	private static AttributeData GetContentAssemblyAttribute(IAssemblySymbol assembly, Compilation compilation)
	{
        var contentAssemblySymbol = compilation.GetTypeByMetadataName(ContentAssemblyAttribute)
        ?? throw new InvalidOperationException("Cannot find ContentAssemblyAttribute type");

		return assembly.GetAttribute(contentAssemblySymbol)
			?? throw new InvalidOperationException($"{assembly.Name} is does not have {ContentAssemblyAttribute}");
	}
}
