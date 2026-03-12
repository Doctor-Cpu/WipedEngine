using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using Wiped.Roslyn;

namespace Wiped.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DiskDefinitionAnalyzer : DiagnosticAnalyzer
{
	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [DiskDefinitionMustBeSealed, DiskDefinitionParameterlessCtorRequired, DiskDefinitionAbstractTypeNotAllowed];

	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();

		context.RegisterCompilationStartAction(startContext =>
		{
			var diskDefAttr = startContext.Compilation
				.GetTypeByMetadataName(DiskDefinitionAttribute);

			if (diskDefAttr == null)
				return;

			startContext.RegisterSymbolAction(
				ctx => AnalyzeType(ctx, diskDefAttr),
				SymbolKind.NamedType
			);
		});
	}

	private static void AnalyzeType(SymbolAnalysisContext context, INamedTypeSymbol diskDefinitionAttribute)
	{
		var type = (INamedTypeSymbol)context.Symbol;

		if (type.TypeKind != TypeKind.Class)
			return;

		if (!type.HasAttribute(diskDefinitionAttribute))
			return;

		var location = type.Locations.FirstOrDefault() ?? Location.None;

		if (!type.IsSealed)
		{
			context.ReportDiagnostic(Diagnostic.Create(
				DiskDefinitionMustBeSealed,
				location,
				type.Name
			));
		}

		if (type.IsAbstract)
		{
			context.ReportDiagnostic(Diagnostic.Create(
				DiskDefinitionAbstractTypeNotAllowed,
				location,
				type.Name
			));
		}

		if (!type.InstanceConstructors.Any(c => c.DeclaredAccessibility == Accessibility.Public && c.Parameters.Length == 0))
		{
			context.ReportDiagnostic(Diagnostic.Create(
				DiskDefinitionParameterlessCtorRequired,
				location,
				type.Name
			));
		}
	}
}
