using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Wiped.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DataDefinitionAnalyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor MustBeSealed = new(
        id: "ENG001",
        title: "DataDefinition must be sealed",
        messageFormat: "Type '{0}' marked with [DataDefinition] must be sealed",
        category: "Engine.Data",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor ParameterlessCtorRequired = new(
        id: "ENG002",
        title: "DataDefinition must have a parameterless constructor",
        messageFormat: "Type '{0}' marked with [DataDefinition] must have a public parameterless constructor",
        category: "Engine.Data",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor AbstractTypeNotAllowed = new(
        id: "ENG003",
        title: "DataDefinition cannot be abstract",
        messageFormat: "Type '{0}' marked with [DataDefinition] cannot be abstract",
        category: "Engine.Data",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [MustBeSealed, ParameterlessCtorRequired, AbstractTypeNotAllowed];

	private const string DataDefinitionAttributeName = "Wiped.Shared.Serialization.DataDefinitionAttribute";

	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();

		context.RegisterCompilationStartAction(startContext =>
		{
			var dataDefAttr = startContext.Compilation
				.GetTypeByMetadataName(DataDefinitionAttributeName);

			if (dataDefAttr == null)
				return;

			startContext.RegisterSymbolAction(
				ctx => AnalyzeType(ctx, dataDefAttr),
				SymbolKind.NamedType
			);
		});
	}

	private static void AnalyzeType(SymbolAnalysisContext context, INamedTypeSymbol dataDefinitionAttribute)
	{
		var type = (INamedTypeSymbol)context.Symbol;

		if (type.TypeKind != TypeKind.Class)
			return;

		var hasAttr = type.GetAttributes().Any(a =>
			SymbolEqualityComparer.Default.Equals(
				a.AttributeClass,
				dataDefinitionAttribute));

		if (!hasAttr)
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
