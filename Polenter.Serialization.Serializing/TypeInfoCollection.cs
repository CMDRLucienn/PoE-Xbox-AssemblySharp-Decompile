using System;
using System.Collections.ObjectModel;

namespace Polenter.Serialization.Serializing;

public sealed class TypeInfoCollection : KeyedCollection<Type, TypeInfo>
{
	public TypeInfo TryGetTypeInfo(Type type)
	{
		if (!Contains(type))
		{
			return null;
		}
		return base[type];
	}

	protected override Type GetKeyForItem(TypeInfo item)
	{
		return item.Type;
	}
}
