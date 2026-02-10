using System.Reflection;
using Wiped.Shared.IoC;

namespace Wiped.Shared.Reflection;

public interface IReflectionManager : IManager
{
	IEnumerable<Type> GetAllDerivedTypes<TBase>();

	IEnumerable<KeyValuePair<Type, TAttribute>> GetTypesWithAttribute<TAttribute>(bool allowUnimplementedTypes = false) where TAttribute : Attribute;

	IEnumerable<MemberAttributeInfo<TAttribute>> GetMemberAttributes<TAttribute>(Type type) where TAttribute : Attribute;

	public readonly struct MemberAttributeInfo<TAttribute> where TAttribute : Attribute
	{
		public string Name { get; }
		internal MemberInfo Member { get; }
		public Type Type { get; }
		public TAttribute Attribute { get; }
		
		internal MemberAttributeInfo(string name, MemberInfo member, Type type, TAttribute attribute) 
		{
			Name = name;
			Member = member;
			Type = type;
			Attribute = attribute;
		}
	}
}
