using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;
using Wiped.Roslyn;

namespace Wiped.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MustHaveAttributeAnalyzer : DiagnosticAnalyzer
{
	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [MustHaveAttrIsNotAttr, MustHaveAttrMissing];

	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
		context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
	}

    private static void AnalyzeMethod(SymbolAnalysisContext context)
	{
        var methodSymbol = (IMethodSymbol)context.Symbol;
		var compilation = context.Compilation;

		if (compilation.GetTypeByMetadataName(MustHaveAttributeAttribute) is not { } mustHaveAttr)
			return;

		foreach (var param in methodSymbol.TypeParameters)
		{
			foreach (var attr in GetRequiredAttributes(param, mustHaveAttr))
			{
				if (!attr.IsAttribute(compilation))
				{
					context.ReportDiagnostic(
						Diagnostic.Create(
							MustHaveAttrIsNotAttr,
							param.GetLocation(),
							attr.Name
						)
					);
				}
			}
		}
	}

	private static void AnalyzeInvocation(OperationAnalysisContext context)
	{
		var invocation = (IInvocationOperation)context.Operation;
		var method = invocation.TargetMethod;
		var compilation = context.Compilation;

		if (compilation.GetTypeByMetadataName(MustHaveAttributeAttribute) is not { } mustHaveAttr)
			return;

		for (var i = 0; i < method.TypeParameters.Length; i++)
		{
			var typeParam = method.TypeParameters[i];
			var actualType = method.TypeArguments[i];

			if (actualType is ITypeParameterSymbol actualTypeParam)
			{
				foreach (var requiredAttr in GetRequiredAttributes(typeParam, mustHaveAttr))
				{
					var found = false;

					foreach (var actualRequired in GetRequiredAttributes(actualTypeParam, mustHaveAttr))
					{
						if (SymbolEqualityComparer.Default.Equals(actualRequired, requiredAttr))
						{
							found = true;
							break;
						}
					}

					if (!found)
					{
						context.ReportDiagnostic(
							Diagnostic.Create(
								MustHaveAttrMissing,
								invocation.Syntax.GetLocation(),
								actualTypeParam.Name,
								requiredAttr.Name
							)
						);
					}
				}
			}
			else
			{
				foreach (var attr in GetRequiredAttributes(typeParam, mustHaveAttr))
				{
					if (!actualType.HasAttribute(attr))
					{
						context.ReportDiagnostic(
							Diagnostic.Create(
								MustHaveAttrMissing,
								invocation.Syntax.GetLocation(),
								actualType.Name,
								attr.Name
							)
						);
					}
				}
			}
		}
	}

	private static IEnumerable<INamedTypeSymbol> GetRequiredAttributes(ISymbol typeParam, INamedTypeSymbol mustHaveAttr)
	{
		foreach (var attr in typeParam.GetAllAttributes(mustHaveAttr))
		{
			foreach (var constant in attr.ConstructorArguments[0].Values)
			{
				if (constant.Value is INamedTypeSymbol requiredAttr)
					yield return requiredAttr;
			}
		}
	}
}
