using Microsoft.CodeAnalysis;
using System.Text;
using Wiped.Roslyn;

namespace Wiped.Generators;

public sealed partial class ModuleGenerator : IIncrementalGenerator
{
	private static void BuildIoC(StringBuilder sb, Compilation compilation)
	{
		sb.AppendLine(
			"""
				public void RegisterIoC()
				{
			"""
		);

		if (compilation.GetTypeByMetadataName(AutoBindAttribute) is not { } autoBindSymbol)
			goto end;

		foreach (var type in compilation.Assembly.GlobalNamespace.GetAllTypes())
		{
			if (type.TypeKind != TypeKind.Class)
				continue;

			if (type.GetAttribute(autoBindSymbol) is not { } attr)
				continue;

			foreach (var serviceSymbol in GetServiceTypeSymbols(attr))
			{
				var serviceName = serviceSymbol.ToDisplayString();
				var implName = type.ToDisplayString();

				sb.AppendLine(
					$"""
							IoCManager.Bind<{serviceName}, {implName}>();
					"""
				);
			}
		}

		end:

		sb.AppendLine(
			"""
				}

			"""
		);
	}

	private static IEnumerable<INamedTypeSymbol> GetServiceTypeSymbols(AttributeData attr)
	{
		foreach (var arg in attr.ConstructorArguments)
		{
			if (arg.Kind == TypedConstantKind.Array)
			{
				// multiple services
				foreach (var item in arg.Values)
				{
					if (item.Value is INamedTypeSymbol sym)
						yield return sym;
				}
			}
			else if (arg.Value is INamedTypeSymbol sym)
			{
				yield return sym;
			}
		}
	}
}
