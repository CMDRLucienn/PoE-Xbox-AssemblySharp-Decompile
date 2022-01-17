using System;
using System.Collections;
using UnityEngine;

namespace Polenter.Serialization.Core;

internal static class Tools
{
	public static bool IsSimple(Type type)
	{
		if (type == typeof(string))
		{
			return true;
		}
		if (type == typeof(DateTime))
		{
			return true;
		}
		if (type == typeof(TimeSpan))
		{
			return true;
		}
		if (type == typeof(decimal))
		{
			return true;
		}
		if (type == typeof(Guid))
		{
			return true;
		}
		if (type == typeof(Type) || type.IsSubclassOf(typeof(Type)))
		{
			return true;
		}
		if (type.IsEnum)
		{
			return true;
		}
		if (type == typeof(byte[]))
		{
			return true;
		}
		return type.IsPrimitive;
	}

	public static bool IsEnumerable(Type type)
	{
		return typeof(IEnumerable).IsAssignableFrom(type);
	}

	public static bool IsCollection(Type type)
	{
		return typeof(ICollection).IsAssignableFrom(type);
	}

	public static bool IsDictionary(Type type)
	{
		return typeof(IDictionary).IsAssignableFrom(type);
	}

	public static bool IsArray(Type type)
	{
		return type.IsArray;
	}

	public static bool WasConvertedToString(Type t)
	{
		if (t == null)
		{
			return false;
		}
		if (t == typeof(MonoBehaviour) || t == typeof(GameObject))
		{
			return true;
		}
		if (t.BaseType != null)
		{
			return WasConvertedToString(t.BaseType);
		}
		return false;
	}

	public static object CreateInstance(Type type)
	{
		if (type == null)
		{
			return null;
		}
		try
		{
			object obj;
			if (type == typeof(Team))
			{
				obj = Team.Create();
			}
			else
			{
				obj = Activator.CreateInstance(type);
				if (obj is UnityEngine.Object)
				{
					Debug.LogError("Someone serialized a " + type.ToString() + " during a save! This won't work, it needs a converter.");
				}
			}
			return obj;
		}
		catch (Exception innerException)
		{
			throw new CreatingInstanceException($"Error during creating an object. Please check if the type \"{type.AssemblyQualifiedName}\" has public parameterless constructor, or if the settings IncludeAssemblyVersionInTypeName, IncludeCultureInTypeName, IncludePublicKeyTokenInTypeName are set to true. Details are in the inner exception.", innerException);
		}
	}
}
