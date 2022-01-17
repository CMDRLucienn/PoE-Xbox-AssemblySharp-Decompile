using System;
using System.Collections.Generic;
using System.Reflection;
using Polenter.Serialization.Serializing;

namespace Polenter.Serialization.Advanced;

public class PropertyProvider
{
	private PropertiesToIgnore _propertiesToIgnore;

	private IList<Type> _attributesToIgnore;

	[ThreadStatic]
	private static PropertyCache _cache;

	public PropertiesToIgnore PropertiesToIgnore
	{
		get
		{
			if (_propertiesToIgnore == null)
			{
				_propertiesToIgnore = new PropertiesToIgnore();
			}
			return _propertiesToIgnore;
		}
		set
		{
			_propertiesToIgnore = value;
		}
	}

	public IList<Type> AttributesToIgnore
	{
		get
		{
			if (_attributesToIgnore == null)
			{
				_attributesToIgnore = new List<Type>();
			}
			return _attributesToIgnore;
		}
		set
		{
			_attributesToIgnore = value;
		}
	}

	private static PropertyCache Cache
	{
		get
		{
			if (_cache == null)
			{
				_cache = new PropertyCache();
			}
			return _cache;
		}
	}

	public IList<PropertyInfo> GetProperties(Polenter.Serialization.Serializing.TypeInfo typeInfo)
	{
		IList<PropertyInfo> list = Cache.TryGetPropertyInfos(typeInfo.Type);
		if (list != null)
		{
			return list;
		}
		PropertyInfo[] allProperties = GetAllProperties(typeInfo.Type);
		List<PropertyInfo> list2 = new List<PropertyInfo>();
		PropertyInfo[] array = allProperties;
		foreach (PropertyInfo propertyInfo in array)
		{
			if (!IgnoreProperty(typeInfo, propertyInfo))
			{
				list2.Add(propertyInfo);
			}
		}
		Cache.Add(typeInfo.Type, list2);
		return list2;
	}

	protected virtual bool IgnoreProperty(Polenter.Serialization.Serializing.TypeInfo info, PropertyInfo property)
	{
		if (PropertiesToIgnore.Contains(info.Type, property.Name))
		{
			return true;
		}
		if (ContainsExcludeFromSerializationAttribute(property))
		{
			return true;
		}
		if (!property.CanRead || !property.CanWrite)
		{
			return true;
		}
		if (property.GetIndexParameters().Length != 0)
		{
			return true;
		}
		return false;
	}

	protected bool ContainsExcludeFromSerializationAttribute(ICustomAttributeProvider property)
	{
		foreach (Type item in AttributesToIgnore)
		{
			if (property.GetCustomAttributes(item, inherit: false).Length != 0)
			{
				return true;
			}
		}
		return false;
	}

	protected virtual PropertyInfo[] GetAllProperties(Type type)
	{
		return type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
	}
}
