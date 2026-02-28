using Wiped.Shared.Localization;

namespace Wiped.Shared.Serialization.Schema;

public sealed class SerializedType
{
	public Type ClrType { get; }
	public TextLocId Name { get; }
	public IReadOnlyList<SerializedField> Fields { get; }
	public bool IsPolymorphic { get; }

	internal SerializedType(Type clrType, TextLocId name, IReadOnlyList<SerializedField> fields, bool isPolymorphic)
	{
		ClrType = clrType;
		Name = name;
		Fields = fields;
		IsPolymorphic = isPolymorphic;
	}
}
