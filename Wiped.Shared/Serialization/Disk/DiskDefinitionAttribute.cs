using Wiped.Shared.Localization.Text;
using Wiped.Shared.Reflection;

namespace Wiped.Shared.Serialization.Disk;

[ReflectableAttributeUsage]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public sealed class DiskDefinitionAttribute(string? editorName = null, string? editorDesc = null) : Attribute // cant use the actual type due to c# fuckery. dont @ me
{
    public TextLocId? EditorName { get; } = editorName;
	public TextLocId? EditorDescription { get; } = editorDesc;
}
