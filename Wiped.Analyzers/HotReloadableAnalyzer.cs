using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using Wiped.Roslyn;

namespace Wiped.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class HotReloadableAnalyzer : DiagnosticAnalyzer
{
	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [HotReloadableAfterNotReloadable, HotReloadableBeforeNotReloadable];

	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeProperty, SyntaxKind.PropertyDeclaration);
	}

	private const string BeforeProperty = "Before";
	private const string AfterProperty = "After";

    private static void AnalyzeProperty(SyntaxNodeAnalysisContext context)
	{
		var property = (PropertyDeclarationSyntax)context.Node;
		var semanticModel = context.SemanticModel;
		var compilation = context.Compilation;

		if (semanticModel.GetDeclaredSymbol(property) is not { } propertySymbol)
			return;

		var containingType = propertySymbol.ContainingType;

		if (property.Identifier.Text is not (BeforeProperty or AfterProperty)) // does it use ordered initialization?
			return;

		if (propertySymbol.Type is not IArrayTypeSymbol) // someone has overridden the before or after field (why would you do that is beyond me)
			return;

		var iHotReloadableInterface = compilation.GetTypeByMetadataName(IHotReloadable);
		var iManagerInterface = compilation.GetTypeByMetadataName(IManager);

		if (iHotReloadableInterface == null && iManagerInterface == null) // nothing can be hot reloadable then
			return;

		if (!IsHotReloadable(containingType, iHotReloadableInterface, iManagerInterface)) // only validate hot reloadable types themselves
			return;

		// walk all types
		foreach (var typeOf in property.DescendantNodes().OfType<TypeOfExpressionSyntax>())
		{
			var typeInfo = semanticModel.GetTypeInfo(typeOf.Type);

			if (typeInfo.Type is not INamedTypeSymbol typeSymbol)
				continue;

			if (IsHotReloadable(typeSymbol, iHotReloadableInterface, iManagerInterface))
				continue;

			var descriptor = property.Identifier.Text == AfterProperty
			 	? HotReloadableAfterNotReloadable
				: HotReloadableBeforeNotReloadable;

			context.ReportDiagnostic(
				Diagnostic.Create(
					descriptor,
					typeOf.GetLocation(),
					typeSymbol.Name,
					containingType.Name
				)
			);
		}
	}

	private static bool IsHotReloadable(ITypeSymbol symbol, INamedTypeSymbol? iHotReloadableInterface, INamedTypeSymbol? iManagerInterface)
	{
		if (iHotReloadableInterface != null)
		{
			if (symbol.ImplementsInterface(iHotReloadableInterface))
				return true;
		}

		if (iManagerInterface != null)
		{
			if (symbol.ImplementsInterface(iManagerInterface))
				return true;
		}

		return false;
	}
}
