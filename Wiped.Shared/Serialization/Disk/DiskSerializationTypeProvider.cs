using Wiped.Shared.IoC;
using Wiped.Shared.Reflection;
using Wiped.Shared.Serialization.Schema;

namespace Wiped.Shared.Serialization.Disk;

internal sealed class DiskSerializationTypeProvider : ISerializedTypeProvider
{
	[Dependency] private readonly IoCDynamic<IReflectionManager> _reflection = default!;

    public IEnumerable<SerializedType> GetSerializedTypes()
	{
		foreach (var (defType, defAttr) in _reflection.Value.GetTypesWithAttribute<DiskDefinitionAttribute>(true))
		{
			List<SerializedField> fields = [];

			foreach (var fieldInfo in _reflection.Value.GetMemberAttributes<DiskFieldAttribute>(defType))
			{
				var field = new SerializedField(
					fieldInfo.Attribute.EditorName ?? fieldInfo.Name,
					fieldInfo.Type,
					fieldInfo.Attribute.Required
				);

				fields.Add(field);
			}

			yield return new SerializedType(
				defType,
				defAttr.EditorName ?? defType.Name,
				fields,
				defType.IsInterface || defType.IsAbstract
			);
		}
	}

	public DiskSerializationTypeProvider()
	{
		IoCManager.ResolveDependencies(this);
	}
}
