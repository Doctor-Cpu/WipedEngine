using Microsoft.CodeAnalysis;

namespace Wiped.Analyzers;

public static class Diagnostics
{
	public static readonly DiagnosticDescriptor MissingAutoBindInterface = new(
		id: "ENG001",
		title: "AutoBind interface not implemented",
		messageFormat: "Type '{0}' is marked with [AutoBind] but does not implement interface '{1}'",
		category: "Engine.IoC",
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true
	);

	public static readonly DiagnosticDescriptor AutoBindMissing = new(
		id: "ENG002",
		title: "IManager interface specified but no AutoBind attribute",
		messageFormat: "IManager interface '{1}' specified but no AutoBind attribute on '{0}'",
		category: "Engine.IoC",
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true
	);

	public static readonly DiagnosticDescriptor AutoBindInterfaceMustBeIManager = new(
		id: "ENG003",
		title: "AutoBind interfaces must implement IManager",
		messageFormat: "Interface '{0}' specified in [AutoBind] must implement IManager",
		category: "Engine.IoC",
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true
	);

	public static readonly DiagnosticDescriptor SwitchableInterfaceMissing = new(
		id: "ENG004",
		title: "Switchable manager selector specified a manager that is not marked [SwitchableManager]",
		messageFormat: "Switchable manager selector '{0}' specified a manager that is not marked [SwitchableManager]",
		category: "Engine.IoC",
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true
	);

	public static readonly DiagnosticDescriptor SwitchableImplementationMissing = new(
		id: "ENG005",
		title: "Children of a [SwitchableManager] must be martked with [SwitchableManagerSelector]",
		messageFormat: "Class '{0}' must be marked with [SwitchableManagerSelector] due to its parent '{1}' being marked with [SwitchableManager]",
		category: "Engine.IoC",
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true
	);

	public static readonly DiagnosticDescriptor SwitchableTypeMismatch = new(
		id: "ENG006",
		title: "Switchable manager selector type mismatch",
		messageFormat: "Class '{0}' specifies a selector that does not match the interface selector type",
		category: "Engine.IoC",
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true
	);

	public static readonly DiagnosticDescriptor SwitchableInterfaceNotImplemented = new(
		id: "ENG007",
		title: "Switchable selector references non-implemented interface",
		messageFormat: "Class '{0}' specifies a selector for interface '{1}' but does not implement it",
		category: "Engine.IoC",
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true
	);

	public static readonly DiagnosticDescriptor SwitchableDuplicateSelector = new(
		id: "ENG008",
    	title: "Duplicate switchable manager selector",
		messageFormat: "Selector '{2}' for interface '{1}' is already implemented by another type; '{0}' cannot use the same selector",
		category: "Engine.IoC",
		defaultSeverity: DiagnosticSeverity.Error,
		customTags: ["CompilationEnd"],
		isEnabledByDefault: true
	);

	public static readonly DiagnosticDescriptor ReflectableBaseUsageMustBeUnsealed = new(
		id: "ENG009",
    	title: "ReflectableBaseUsage applied to a sealed class",
		messageFormat: "Type '{0}' has the ReflectableBaseUsage attribute when it cannot be inherited",
		category: "Engine.Reflection",
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true
	);

	public static readonly DiagnosticDescriptor ReflectableAttrUsageOnNonAttr = new(
		id: "ENG010",
    	title: "ReflectableAttributeUsage applied to a non-attribute",
		messageFormat: "Type '{0}' has the ReflectableAttributeUsage attribute when it is not an attribute itself",
		category: "Engine.Reflection",
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true
	);

	public static readonly DiagnosticDescriptor MustHaveAttrIsNotAttr = new(
		id: "ENG011",
    	title: "MustHaveAttribute requires an attribute type",
        messageFormat: "Type '{0}' used in MustHaveAttribute must be an attribute",
		category: "Engine.Diagnostics",
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true
	);

	public static readonly DiagnosticDescriptor MustHaveAttrMissing = new(
		id: "ENG012",
		title: "Missing required attribute",
		messageFormat: "Type '{0}' must have attribute '{1}' to be used as this generic parameter",
		category: "Engine.Diagnostics",
		defaultSeverity: DiagnosticSeverity.Error,
		isEnabledByDefault: true
	);
}
