using Wiped.Shared.Localization;

namespace Wiped.Shared.Serialization.Disk;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class DiskFieldAttribute(string? editorName = null, string? editorDesc = null, bool required = false) : Attribute // cant use the actual type due to c# fuckery. dont @ me
{
    public TextLocId? EditorName { get; } = editorName;
	public TextLocId? EditorDescription { get; } = editorDesc;
	public bool Required { get; } = required;
}
