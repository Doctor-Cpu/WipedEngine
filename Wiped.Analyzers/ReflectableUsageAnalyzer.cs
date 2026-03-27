using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using Wiped.Roslyn;

namespace Wiped.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ReflectableUsageAnalyzer : DiagnosticAnalyzer
{
	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [ReflectableBaseUsageMustBeUnsealed, ReflectableAttrUsageOnNonAttr];

	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
	}

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
	{
		AnalyzeReflectableBaseUsage(context);
		AnalyzeReflectableAttributeUsage(context);
	}

	private static void AnalyzeReflectableBaseUsage(SymbolAnalysisContext context)
	{
        var typeSymbol = (INamedTypeSymbol)context.Symbol;
		var compilation = context.Compilation;

		if (compilation.GetTypeByMetadataName(ReflectableBaseUsageAttribute) is not { } reflectableBaseUsage)
			return;

		if (!typeSymbol.HasAttribute(reflectableBaseUsage))
			return;

		if (typeSymbol.IsSealed)
		{
			context.ReportDiagnostic(
				Diagnostic.Create(
					ReflectableBaseUsageMustBeUnsealed,
					typeSymbol.GetLocation(),
					typeSymbol.Name
				)
			);
		}
	}

	private static void AnalyzeReflectableAttributeUsage(SymbolAnalysisContext context)
	{
        var typeSymbol = (INamedTypeSymbol)context.Symbol;
		var compilation = context.Compilation;

		if (compilation.GetTypeByMetadataName(ReflectableAttributeUsageAttribute) is not { } reflectableAttrUsage)
			return;

		if (!typeSymbol.HasAttribute(reflectableAttrUsage))
			return;

		if (!typeSymbol.IsAttribute(compilation))
		{
			context.ReportDiagnostic(
				Diagnostic.Create(
					ReflectableAttrUsageOnNonAttr,
					typeSymbol.GetLocation(),
					typeSymbol.Name
				)
			);
		}
	}
}
