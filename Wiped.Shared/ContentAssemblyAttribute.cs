namespace Wiped.Shared;

[AttributeUsage(AttributeTargets.Assembly)]
public sealed class ContentAssemblyAttribute(string rootNamespace) : Attribute
{
	public string RootNamespace = rootNamespace;
}
