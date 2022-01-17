using System;
using System.Collections.Generic;
using System.Reflection;
using Polenter.Serialization.Core;
using UnityEngine;

namespace Polenter.Serialization.Deserializing;

public sealed class ObjectFactory
{
	private class MultiDimensionalArrayCreatingInfo
	{
		public int[] Lengths { get; set; }

		public int[] LowerBounds { get; set; }
	}

	private readonly object[] _emptyObjectArray = new object[0];

	private readonly Dictionary<int, object> _objectCache = new Dictionary<int, object>();

	public object CreateObject(Property property)
	{
		if (property == null)
		{
			throw new ArgumentNullException("property");
		}
		if (property is NullProperty)
		{
			return null;
		}
		if (property.Type == null)
		{
			throw new InvalidOperationException($"Property type is not defined. Property: \"{property.Name}\"");
		}
		if (property is SimpleProperty property2)
		{
			if (property.ConvertedType != null)
			{
				return createConvertedObject(property as SimpleProperty);
			}
			return createObjectFromSimpleProperty(property2);
		}
		if (property.ConvertedType != null)
		{
			return null;
		}
		if (!(property is ReferenceTargetProperty referenceTargetProperty))
		{
			return null;
		}
		if (referenceTargetProperty.Reference != null && !referenceTargetProperty.Reference.IsProcessed)
		{
			return _objectCache[referenceTargetProperty.Reference.Id];
		}
		object obj = createObjectCore(referenceTargetProperty);
		if (obj != null)
		{
			return obj;
		}
		throw new InvalidOperationException($"Unknown Property type: {property.GetType().Name}");
	}

	private object createConvertedObject(SimpleProperty property)
	{
		object obj = null;
		string text = property.Value.ToString();
		try
		{
			obj = GameResources.LoadPrefab(text, instantiate: false);
			if (obj != null && obj is GameObject)
			{
				Persistence component = (obj as GameObject).GetComponent<Persistence>();
				if ((bool)component)
				{
					component.IsPrefab = true;
				}
				if (property.ConvertedType != obj.GetType())
				{
					obj = (obj as GameObject).GetComponent(property.ConvertedType);
				}
			}
			else if (obj is MonoBehaviour && property.ConvertedType != obj.GetType())
			{
				obj = ((!(property.ConvertedType == typeof(GameObject))) ? ((UnityEngine.Object)(obj as MonoBehaviour).GetComponent(property.ConvertedType)) : ((UnityEngine.Object)(obj as MonoBehaviour).gameObject));
			}
		}
		finally
		{
			if (obj == null)
			{
				Debug.LogError("There was an error during load. The prefab at " + text + " wasn't loaded!");
			}
		}
		return obj;
	}

	private object createObjectCore(ReferenceTargetProperty property)
	{
		if (property is MultiDimensionalArrayProperty property2)
		{
			return createObjectFromMultidimensionalArrayProperty(property2);
		}
		if (property is SingleDimensionalArrayProperty property3)
		{
			return createObjectFromSingleDimensionalArrayProperty(property3);
		}
		if (property is DictionaryProperty property4)
		{
			return createObjectFromDictionaryProperty(property4);
		}
		if (property is CollectionProperty property5)
		{
			return createObjectFromCollectionProperty(property5);
		}
		if (property is ComplexProperty property6)
		{
			return createObjectFromComplexProperty(property6);
		}
		return null;
	}

	private static object createObjectFromSimpleProperty(SimpleProperty property)
	{
		return property.Value;
	}

	private object createObjectFromComplexProperty(ComplexProperty property)
	{
		object obj = Tools.CreateInstance(property.Type);
		if (property.Reference != null)
		{
			_objectCache.Add(property.Reference.Id, obj);
		}
		fillProperties(obj, property.Properties);
		return obj;
	}

	private object createObjectFromCollectionProperty(CollectionProperty property)
	{
		object obj = Tools.CreateInstance(property.Type);
		if (property.Reference != null)
		{
			_objectCache.Add(property.Reference.Id, obj);
		}
		fillProperties(obj, property.Properties);
		MethodInfo method = obj.GetType().GetMethod("Add");
		if (method != null && method.GetParameters().Length == 1)
		{
			Type type = obj.GetType().GetGenericArguments()[0];
			{
				foreach (Property item in property.Items)
				{
					if (Tools.WasConvertedToString(type))
					{
						item.ConvertedType = type;
					}
					object obj2 = CreateObject(item);
					try
					{
						method.Invoke(obj, new object[1] { obj2 });
					}
					catch (Exception ex)
					{
						Debug.LogError(ex.Message);
					}
				}
				return obj;
			}
		}
		return obj;
	}

	private object createObjectFromDictionaryProperty(DictionaryProperty property)
	{
		object obj = Tools.CreateInstance(property.Type);
		if (property.Reference != null)
		{
			_objectCache.Add(property.Reference.Id, obj);
		}
		fillProperties(obj, property.Properties);
		MethodInfo method = obj.GetType().GetMethod("Add");
		if (method != null && method.GetParameters().Length == 2)
		{
			foreach (KeyValueItem item in property.Items)
			{
				object obj2 = CreateObject(item.Key);
				object obj3 = CreateObject(item.Value);
				method.Invoke(obj, new object[2] { obj2, obj3 });
			}
			return obj;
		}
		return obj;
	}

	private void fillProperties(object obj, IEnumerable<Property> properties)
	{
		Type type = obj.GetType();
		foreach (Property property2 in properties)
		{
			try
			{
				object obj2 = CreateObject(property2);
				if (obj2 == null)
				{
					continue;
				}
				PropertyInfo property = type.GetProperty(property2.Name);
				if (property != null)
				{
					if (property2.ConvertedType != null)
					{
						if (property2.ConvertedType != typeof(GameObject) && obj2.GetType() == typeof(GameObject))
						{
							obj2 = (obj2 as GameObject).GetComponent(property.PropertyType);
						}
						else if (property2.ConvertedType != obj2.GetType() && obj2 is MonoBehaviour)
						{
							obj2 = (obj2 as MonoBehaviour).GetComponent(property.PropertyType);
						}
					}
					property.SetValue(obj, obj2, _emptyObjectArray);
				}
				else
				{
					FieldInfo field = type.GetField(property2.Name);
					if (field != null)
					{
						field.SetValue(obj, obj2);
					}
				}
			}
			catch (Exception ex)
			{
				string text = ex.ToString();
				Debug.LogError(property2.Type.ToString() + " :: " + text);
			}
		}
	}

	private object createObjectFromSingleDimensionalArrayProperty(SingleDimensionalArrayProperty property)
	{
		int count = property.Items.Count;
		Array array = createArrayInstance(property.ElementType, new int[1] { count }, new int[1] { property.LowerBound });
		if (property.Reference != null)
		{
			_objectCache.Add(property.Reference.Id, array);
		}
		for (int i = property.LowerBound; i < property.LowerBound + count; i++)
		{
			Property property2 = property.Items[i];
			Type elementType = array.GetType().GetElementType();
			if (Tools.WasConvertedToString(elementType))
			{
				property2.ConvertedType = elementType;
			}
			object obj = CreateObject(property2);
			if (obj != null)
			{
				if (elementType != obj.GetType() && !elementType.IsAssignableFrom(obj.GetType()))
				{
					Debug.LogError(array.ToString() + " expected an element of type " + elementType.ToString() + " but got element of type " + obj.GetType().ToString());
				}
				else
				{
					array.SetValue(obj, i);
				}
			}
		}
		return array;
	}

	private object createObjectFromMultidimensionalArrayProperty(MultiDimensionalArrayProperty property)
	{
		MultiDimensionalArrayCreatingInfo multiDimensionalArrayCreatingInfo = getMultiDimensionalArrayCreatingInfo(property.DimensionInfos);
		Array array = createArrayInstance(property.ElementType, multiDimensionalArrayCreatingInfo.Lengths, multiDimensionalArrayCreatingInfo.LowerBounds);
		if (property.Reference != null)
		{
			_objectCache.Add(property.Reference.Id, array);
		}
		foreach (MultiDimensionalArrayItem item in property.Items)
		{
			object obj = CreateObject(item.Value);
			if (obj != null)
			{
				array.SetValue(obj, item.Indexes);
			}
		}
		return array;
	}

	private static Array createArrayInstance(Type elementType, int[] lengths, int[] lowerBounds)
	{
		return Array.CreateInstance(elementType, lengths, lowerBounds);
	}

	private static MultiDimensionalArrayCreatingInfo getMultiDimensionalArrayCreatingInfo(IEnumerable<DimensionInfo> infos)
	{
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		foreach (DimensionInfo info in infos)
		{
			list.Add(info.Length);
			list2.Add(info.LowerBound);
		}
		return new MultiDimensionalArrayCreatingInfo
		{
			Lengths = list.ToArray(),
			LowerBounds = list2.ToArray()
		};
	}
}
