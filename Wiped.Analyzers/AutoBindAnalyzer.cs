using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Wiped.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AutoBindAnalyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor MustBeAutoBindable = new(
        id: "ENG004",
        title: "IManagers must have the AutoBindAttribute",
        messageFormat: "Type '{0}' must be marked with [AutoBind]",
        category: "Engine.Yaml",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public static readonly DiagnosticDescriptor MustBeIManager = new(
        id: "ENG005",
        title: "AutoBind types must implement IManager",
        messageFormat: "Type '{0}' marked with [AutoBind] must implement IManager",
        category: "Engine.IoC",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

	public static readonly DiagnosticDescriptor MissingAutoBindInterface = new(
		id: "ENG006",
		title: "AutoBind interface not implemented",
		messageFormat: "Type '{0}' is marked with [AutoBind] but does not implement interface '{1}'",
		category: "Engine.IoC",
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true
	);

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [MustBeAutoBindable, MustBeIManager, MissingAutoBindInterface];

	private const string AutoBindAttributeName = "Wiped.Shared.IoC.AutoBindAttribute";
	private const string IManagerName = "Wiped.Shared.IoC.IManager";

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

        var autoBindAttribute = compilation.GetTypeByMetadataName(AutoBindAttributeName);
        var iManagerInterface = compilation.GetTypeByMetadataName(IManagerName);

        if (autoBindAttribute is null || iManagerInterface is null)
            return; // referenced assembly missing

   		var hasAutoBind = typeSymbol.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, autoBindAttribute));
        var implementsIManager = typeSymbol.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, iManagerInterface));

		var location = typeSymbol.Locations.FirstOrDefault() ?? Location.None;

		if (implementsIManager && !hasAutoBind)
		{
			context.ReportDiagnostic(Diagnostic.Create(
				MustBeAutoBindable,
				location,
				typeSymbol.Name
			));
		}

		if (hasAutoBind && !implementsIManager)
		{
			context.ReportDiagnostic(Diagnostic.Create(
				MustBeIManager,
				location,
				typeSymbol.Name
			));
		}

		if (!hasAutoBind)
			return;

		var declaredInterfaces = GetInterfacesDeclaredInAutoBind(typeSymbol, autoBindAttribute);

		foreach (var declaredInterface in declaredInterfaces)
		{
			var isImplemented = typeSymbol.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, declaredInterface));

			if (isImplemented)
				continue;

			context.ReportDiagnostic(Diagnostic.Create(
				MissingAutoBindInterface,
				location,
				typeSymbol.Name,
				declaredInterface.ToDisplayString()
			));
		}
	}

	private static ImmutableArray<INamedTypeSymbol> GetInterfacesDeclaredInAutoBind(INamedTypeSymbol typeSymbol, INamedTypeSymbol autoBindAttribute)
	{
		var builder = ImmutableArray.CreateBuilder<INamedTypeSymbol>();

		foreach (var attribute in typeSymbol.GetAttributes())
		{
			// only process AutoBind attributes
			if (!SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, autoBindAttribute))
				continue;

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
