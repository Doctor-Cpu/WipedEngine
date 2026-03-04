using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Wiped.Roslyn;

namespace Wiped.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AutoBindAnalyzer : DiagnosticAnalyzer
{
	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [MissingAutoBindInterface, AutoBindMissing, AutoBindInterfaceMustBeIManager];

	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
	}

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
	{
        var typeSymbol = (INamedTypeSymbol)context.Symbol;
		if (typeSymbol.IsAbstract || typeSymbol.TypeKind != TypeKind.Class)
			return;

		var compilation = context.Compilation;

        if (compilation.GetTypeByMetadataName(AutoBindAttribute) is not { } autoBindAttribute ||
			compilation.GetTypeByMetadataName(IManager) is not { } iManagerInterface)
		{
            return; // referenced assembly missing
		}

		if (!typeSymbol.HasAttribute(autoBindAttribute))
		{
			var implementedManagerInterfaces = GetImplementedInterfaces(typeSymbol, iManagerInterface);
			if (!implementedManagerInterfaces.Any())
				return;

			foreach (var iface in implementedManagerInterfaces)
			{
				context.ReportDiagnostic(
					Diagnostic.Create(
						AutoBindMissing,
						typeSymbol.GetLocation(),
						typeSymbol.Name,
						iface.ToDisplayString()
					)
				);
			}

			return;
		}

		var declaredInterfaces = GetInterfacesDeclaredInAutoBind(typeSymbol, autoBindAttribute);

		foreach (var declaredInterface in declaredInterfaces)
		{
			var isImplemented = typeSymbol.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, declaredInterface));
			if (!isImplemented)
			{
				context.ReportDiagnostic(
					Diagnostic.Create(
						MissingAutoBindInterface,
						typeSymbol.GetLocation(),
						typeSymbol.Name,
						declaredInterface.ToDisplayString()
					)
				);
			}

			var interfaceImplementsIManager = declaredInterface.ImplementsInterface(iManagerInterface);
			if (!interfaceImplementsIManager)
			{
				context.ReportDiagnostic(
					Diagnostic.Create(
						AutoBindInterfaceMustBeIManager,
						declaredInterface.GetLocation(),
						declaredInterface.ToDisplayString()
					)
				);
			}
		}
	}

	private static ImmutableArray<INamedTypeSymbol> GetImplementedInterfaces(INamedTypeSymbol typeSymbol, INamedTypeSymbol iManagerInterface)
	{
		return [.. typeSymbol.AllInterfaces.Where(i => i.ImplementsInterface(iManagerInterface))];
	}

	private static ImmutableArray<INamedTypeSymbol> GetInterfacesDeclaredInAutoBind(INamedTypeSymbol typeSymbol, INamedTypeSymbol autoBindAttribute)
	{
		var builder = ImmutableArray.CreateBuilder<INamedTypeSymbol>();

		if (typeSymbol.GetAttribute(autoBindAttribute) is { } attribute)
		{
			// extract the services from the attributes constructor arguments
			foreach (var argument in attribute.ConstructorArguments)
				AddInterfacesFromArgument(argument, builder);
		}

		return builder.ToImmutable();
	}

	private static void AddInterfacesFromArgument(TypedConstant argument, ImmutableArray<INamedTypeSymbol>.Builder builder)
	{
		if (argument.Kind != TypedConstantKind.Array)
			return; // not a param array

		foreach (var value in argument.Values)
		{
			if (value.Value is INamedTypeSymbol iface && iface.TypeKind == TypeKind.Interface)
				builder.Add(iface);
		}
	}
}
