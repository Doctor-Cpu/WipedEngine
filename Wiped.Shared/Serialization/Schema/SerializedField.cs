using Wiped.Shared.Localization.Text;

namespace Wiped.Shared.Serialization.Schema;

public sealed class SerializedField
{
	public TextLocId Name { get; }
	public Type FieldType { get; }
	public bool Required { get; }

	internal SerializedField(TextLocId name, Type fieldType, bool required)
	{
		Name = name;
		FieldType = fieldType;
		Required = required;
	}
}
