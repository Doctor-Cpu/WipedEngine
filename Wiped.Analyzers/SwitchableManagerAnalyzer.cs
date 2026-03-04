using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using Wiped.Roslyn;

namespace Wiped.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SwitchableManagerAnalyzer : DiagnosticAnalyzer
{
	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [SwitchableInterfaceMissing, SwitchableImplementationMissing, SwitchableTypeMismatch, SwitchableInterfaceNotImplemented, SwitchableDuplicateSelector];

	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();
		context.RegisterCompilationStartAction(RegisterDuplicateSelectorAnalysis);
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
	}

	private static void RegisterDuplicateSelectorAnalysis(CompilationStartAnalysisContext context)
	{
		var compilation = context.Compilation;
		if (compilation.GetTypeByMetadataName(SwitchableManagerSelectorAttribute) is not { } switchableSelectorAttr)
			return;

		var map = new ConcurrentDictionary<(INamedTypeSymbol Interface, object SelectorValue, string SelectorName), List<INamedTypeSymbol>>(new InterfaceSelectorComparer());

		context.RegisterSymbolAction(ctx => CollectSelectorRegistrations(ctx, switchableSelectorAttr, map), SymbolKind.NamedType);
		context.RegisterCompilationEndAction(ctx => ReportDuplicateSelectors(ctx, map));
	}

	private static void CollectSelectorRegistrations(SymbolAnalysisContext context, INamedTypeSymbol selectorAttr, ConcurrentDictionary<(INamedTypeSymbol Interface, object SelectorValue, string SelectorName), List<INamedTypeSymbol>> map)
	{
		var type = (INamedTypeSymbol)context.Symbol;

		if (type.TypeKind != TypeKind.Class)
			return;

		foreach (var attr in type.GetAllAttributes(selectorAttr))
		{
			if (attr.AttributeClass is not INamedTypeSymbol attrClass)
				continue;

			var interfaceType = (INamedTypeSymbol)attrClass.TypeArguments[0];
			var selectorValue = attr.ConstructorArguments[0].Value!;
    		var selectorType = (INamedTypeSymbol)attrClass.TypeArguments[1];

			var selectorName = selectorType.GetEnumName(selectorValue);

			var key = (interfaceType, selectorValue, selectorName);

			var list = map.GetOrAdd(key, []);
			lock (list)
			{
				list.Add(type);
			}
		}
	}

	private static void ReportDuplicateSelectors(CompilationAnalysisContext context, ConcurrentDictionary<(INamedTypeSymbol Interface, object SelectorValue, string SelectorName), List<INamedTypeSymbol>> map)
	{
		foreach (var entry in map)
		{
			var (iface, selector, selectorName) = entry.Key;
			var implementations = entry.Value;

			if (implementations.Count <= 1)
				continue;

			foreach (var impl in implementations)
			{
				context.ReportDiagnostic(
					Diagnostic.Create(
						SwitchableDuplicateSelector,
						impl.GetLocation(),
						impl.Name,
						iface.Name,
						selectorName
					)
				);
			}
		}
	}

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
	{
        var typeSymbol = (INamedTypeSymbol)context.Symbol;
		var compilation = context.Compilation;

		if (typeSymbol.TypeKind != TypeKind.Class)
			return;

		if (compilation.GetTypeByMetadataName(SwitchableManagerAttribute) is not { } switchableManagerAttr ||
			compilation.GetTypeByMetadataName(SwitchableManagerSelectorAttribute) is not { } switchableSelectorAttr)
		{
			return;
		}

		var selectors = typeSymbol.GetAllAttributes(switchableSelectorAttr).ToList();

		foreach (var iface in typeSymbol.AllInterfaces)
		{
			if (!iface.HasAttribute(switchableManagerAttr))
				continue;

			if (!selectors.Any(s =>
				{
					if (s.AttributeClass is not INamedTypeSymbol attrClass)
					return false;

					return SymbolEqualityComparer.Default.Equals(attrClass.TypeArguments[0], iface);
				}
			))
			{
				context.ReportDiagnostic(
					Diagnostic.Create(
						SwitchableImplementationMissing,
						typeSymbol.GetLocation(),
						typeSymbol.Name,
						iface.Name
					)
				);
			}
		}

		foreach (var selector in selectors)
		{
			if (selector.AttributeClass is not INamedTypeSymbol attrClass)
				continue;

			var ifaceTypeArg = attrClass.TypeArguments[0];
			var selectorTypeArg = attrClass.TypeArguments[1];

			if (!typeSymbol.AllInterfaces.Contains(ifaceTypeArg, SymbolEqualityComparer.Default))
			{
				context.ReportDiagnostic(
					Diagnostic.Create(
						SwitchableInterfaceNotImplemented,
						typeSymbol.GetLocation(),
						typeSymbol.Name,
						typeSymbol.Name,
						ifaceTypeArg.Name
					)
				);
			}

			var ifaceSwitchables = ifaceTypeArg.GetAllAttributes(switchableManagerAttr);
			if (!ifaceSwitchables.Any())
			{
				context.ReportDiagnostic(
					Diagnostic.Create(
						SwitchableInterfaceMissing,
						typeSymbol.GetLocation(),
						typeSymbol.Name
					)
				);

				continue;
			}

			foreach (var ifaceSwitchable in ifaceSwitchables)
			{
				if (ifaceSwitchable.AttributeClass is not INamedTypeSymbol ifaceAttrClass || !SymbolEqualityComparer.Default.Equals(selectorTypeArg, ifaceAttrClass.TypeArguments[0]))
				{
					context.ReportDiagnostic(
						Diagnostic.Create(
							SwitchableTypeMismatch,
							typeSymbol.GetLocation(),
							typeSymbol.Name
						)
					);
				}
			}
		}
	}

	private sealed class InterfaceSelectorComparer : IEqualityComparer<(INamedTypeSymbol Interface, object SelectorValue, string SelectorName)>
	{
		public bool Equals((INamedTypeSymbol Interface, object SelectorValue, string SelectorName) x, (INamedTypeSymbol Interface, object SelectorValue, string SelectorName) y)
		{
			// dont compare the name as its just there for diagnostic reporting
			return SymbolEqualityComparer.Default.Equals(x.Interface, y.Interface)
				   && Equals(x.SelectorValue, y.SelectorValue);
		}

		public int GetHashCode((INamedTypeSymbol Interface, object SelectorValue, string SelectorName) obj)
		{
			unchecked
			{
			// dont hash the name as its just there for diagnostic reporting
				var hash = SymbolEqualityComparer.Default.GetHashCode(obj.Interface);
				hash ^= obj.SelectorValue?.GetHashCode() ?? 0;
				return hash;
			}
		}
	}
}
