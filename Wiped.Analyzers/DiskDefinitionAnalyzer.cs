using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using Wiped.Roslyn;

namespace Wiped.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DiskDefinitionAnalyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor MustBeSealed = new(
        id: "ENG001",
        title: "DiskDefinition must be sealed",
        messageFormat: "Type '{0}' marked with [DiskDefinition] must be sealed",
        category: "Engine.Disk",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor ParameterlessCtorRequired = new(
        id: "ENG002",
        title: "DiskDefinition must have a parameterless constructor",
        messageFormat: "Type '{0}' marked with [DiskDefinition] must have a public parameterless constructor",
        category: "Engine.Disk",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor AbstractTypeNotAllowed = new(
        id: "ENG003",
        title: "DiskDefinition cannot be abstract",
        messageFormat: "Type '{0}' marked with [DiskDefinition] cannot be abstract",
        category: "Engine.Disk",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [MustBeSealed, ParameterlessCtorRequired, AbstractTypeNotAllowed];

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
				MustBeSealed,
				location,
				type.Name
			));
		}

		if (type.IsAbstract)
		{
			context.ReportDiagnostic(Diagnostic.Create(
				AbstractTypeNotAllowed,
				location,
				type.Name
			));
		}

		if (!type.InstanceConstructors.Any(c => c.DeclaredAccessibility == Accessibility.Public && c.Parameters.Length == 0))
		{
			context.ReportDiagnostic(Diagnostic.Create(
				ParameterlessCtorRequired,
				location,
				type.Name
			));
		}
	}
}
