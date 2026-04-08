namespace Wiped.Shared;

[AttributeUsage(AttributeTargets.Assembly)]
internal sealed class EngineAssemblyAttribute(string rootNamespace) : Attribute, IWipedAssemblyAttribute
{
	string IWipedAssemblyAttribute.RootNamespace => rootNamespace;
}
