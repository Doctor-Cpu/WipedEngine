using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Wiped.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AutoBindAnalyzer : DiagnosticAnalyzer
{
	public static readonly DiagnosticDescriptor MissingAutoBindInterface = new(
		id: "ENG004",
		title: "AutoBind interface not implemented",
		messageFormat: "Type '{0}' is marked with [AutoBind] but does not implement interface '{1}'",
		category: "Engine.IoC",
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true
	);

	public static readonly DiagnosticDescriptor AutoBindMissing = new(
		id: "ENG005",
		title: "IManager interface specified but no AutoBind attribute",
		messageFormat: "IManager interface '{1}' specified but no AutoBind attribute on '{0}'",
		category: "Engine.IoC",
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true
	);

	public static readonly DiagnosticDescriptor AutoBindInterfaceMustBeIManager = new(
		id: "ENG006",
		title: "AutoBind interfaces must implement IManager",
		messageFormat: "Interface '{0}' specified in [AutoBind] must implement IManager",
		category: "Engine.IoC",
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true
	);

	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [MissingAutoBindInterface, AutoBindMissing, AutoBindInterfaceMustBeIManager];

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

		var implementationLocation = typeSymbol.Locations.FirstOrDefault() ?? Location.None;

   		var hasAutoBind = typeSymbol.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, autoBindAttribute));
		if (!hasAutoBind)
		{
			var implementedManagerInterfaces = GetImplementedInterfaces(typeSymbol, i => IsManagerInterface(i, iManagerInterface));
			if (!implementedManagerInterfaces.Any())
				return;

			foreach (var iface in implementedManagerInterfaces)
			{
				context.ReportDiagnostic(Diagnostic.Create(
					AutoBindMissing,
					implementationLocation,
					typeSymbol.Name,
					iface.ToDisplayString()
				));
			}

			return;
		}

		var declaredInterfaces = GetInterfacesDeclaredInAutoBind(typeSymbol, autoBindAttribute);

		foreach (var declaredInterface in declaredInterfaces)
		{
			var isImplemented = typeSymbol.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, declaredInterface));
			if (!isImplemented)
			{
				context.ReportDiagnostic(Diagnostic.Create(
					MissingAutoBindInterface,
					implementationLocation,
					typeSymbol.Name,
					declaredInterface.ToDisplayString()
				));
			}

			var interfaceLocation = declaredInterface.Locations.FirstOrDefault() ?? Location.None;

			var interfaceImplementsIManager = IsManagerInterface(declaredInterface, iManagerInterface);
			if (!interfaceImplementsIManager)
			{
				context.ReportDiagnostic(Diagnostic.Create(
					AutoBindInterfaceMustBeIManager,
					interfaceLocation,
					declaredInterface.ToDisplayString()
				));
			}
		}
	}

	private static bool IsManagerInterface(INamedTypeSymbol iface, INamedTypeSymbol iManager)
	{
		if (SymbolEqualityComparer.Default.Equals(iface, iManager))
			return true;

		return iface.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, iManager));
	}

	private static ImmutableArray<INamedTypeSymbol> GetImplementedInterfaces(INamedTypeSymbol typeSymbol, Func<INamedTypeSymbol, bool> func)
	{
		return [.. typeSymbol.AllInterfaces.Where(i => func(i))];
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
