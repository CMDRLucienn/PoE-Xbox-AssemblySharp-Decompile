using System;
using Polenter.Serialization.Core;

namespace Polenter.Serialization.Serializing;

public sealed class TypeInfo
{
	[ThreadStatic]
	private static TypeInfoCollection _cache;

	public bool IsSimple { get; set; }

	public bool IsArray { get; set; }

	public bool IsEnumerable { get; set; }

	public bool IsCollection { get; set; }

	public bool IsDictionary { get; set; }

	public Type ElementType { get; set; }

	public Type KeyType { get; set; }

	public int ArrayDimensionCount { get; set; }

	public Type Type { get; set; }

	private static TypeInfoCollection Cache
	{
		get
		{
			if (_cache == null)
			{
				_cache = new TypeInfoCollection();
			}
			return _cache;
		}
	}

	public static TypeInfo GetTypeInfo(object obj)
	{
		if (obj == null)
		{
			throw new ArgumentNullException("obj");
		}
		return GetTypeInfo(obj.GetType());
	}

	public static TypeInfo GetTypeInfo(Type type)
	{
		TypeInfo typeInfo = Cache.TryGetTypeInfo(type);
		if (typeInfo == null)
		{
			typeInfo = new TypeInfo();
			typeInfo.Type = type;
			typeInfo.IsSimple = Tools.IsSimple(type);
			if (type == typeof(byte[]))
			{
				typeInfo.ElementType = typeof(byte);
			}
			if (!typeInfo.IsSimple)
			{
				typeInfo.IsArray = Tools.IsArray(type);
				if (typeInfo.IsArray)
				{
					if (type.HasElementType)
					{
						typeInfo.ElementType = type.GetElementType();
					}
					typeInfo.ArrayDimensionCount = type.GetArrayRank();
				}
				else
				{
					typeInfo.IsEnumerable = Tools.IsEnumerable(type);
					if (typeInfo.IsEnumerable)
					{
						typeInfo.IsCollection = Tools.IsCollection(type);
						if (typeInfo.IsCollection)
						{
							typeInfo.IsDictionary = Tools.IsDictionary(type);
							Type type2 = type;
							bool flag;
							do
							{
								flag = fillKeyAndElementType(typeInfo, type2);
								type2 = type2.BaseType;
							}
							while (!flag && type2 != null && type2 != typeof(object));
						}
					}
				}
			}
			Cache.Add(typeInfo);
		}
		return typeInfo;
	}

	private static bool fillKeyAndElementType(TypeInfo typeInfo, Type type)
	{
		if (type.IsGenericType)
		{
			Type[] genericArguments = type.GetGenericArguments();
			if (typeInfo.IsDictionary)
			{
				typeInfo.KeyType = genericArguments[0];
				typeInfo.ElementType = genericArguments[1];
			}
			else
			{
				typeInfo.ElementType = genericArguments[0];
			}
			return genericArguments.Length != 0;
		}
		return false;
	}
}
