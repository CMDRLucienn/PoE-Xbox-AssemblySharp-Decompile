using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class ComponentUtils
{
	private class VariableList
	{
		public List<FieldInfo> variables;

		public List<FieldInfo> lists;

		public List<FieldInfo> dictionarys;

		public VariableList()
		{
			variables = new List<FieldInfo>();
			lists = new List<FieldInfo>();
			dictionarys = new List<FieldInfo>();
		}
	}

	private static List<FieldInfo> s_fieldInfo = new List<FieldInfo>();

	private static List<PropertyInfo> s_propertyInfo = new List<PropertyInfo>();

	private static Type s_typeOfMonoBehavior = typeof(MonoBehaviour);

	private static Type s_typeOfObject = typeof(UnityEngine.Object);

	private static Type s_typeOfMulticastDelegate = typeof(MulticastDelegate);

	private static Type s_typeOfSystemObject = typeof(object);

	private static Type s_typeOfList = typeof(IList);

	private static Type s_typeOfDictionary = typeof(IDictionary);

	private static Type s_typeOfEventHandler = typeof(EventHandler);

	private static MethodInfo s_listClearMethod = typeof(IList).GetMethod("Clear");

	private static MethodInfo s_dictionaryClearMethod = typeof(IDictionary).GetMethod("Clear");

	private static Dictionary<Type, VariableList> s_variableTypes = new Dictionary<Type, VariableList>();

	public static void Cleanup()
	{
		s_variableTypes.Clear();
	}

	public static T GetComponent<T>(GameObject gameObject) where T : Component
	{
		if ((bool)gameObject)
		{
			return gameObject.GetComponent<T>();
		}
		return null;
	}

	public static T GetComponent<T>(Component component) where T : Component
	{
		if ((bool)component)
		{
			return component.GetComponent<T>();
		}
		return null;
	}

	public static void GetAllFields(Type t, ref List<FieldInfo> list)
	{
		if (!(t == null) && !(t == s_typeOfObject) && !(t == s_typeOfMonoBehavior))
		{
			BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			list.AddRange(t.GetFields(bindingAttr));
			GetAllFields(t.BaseType, ref list);
		}
	}

	public static void GetAllProperties(Type t, ref List<PropertyInfo> list)
	{
		if (!(t == null) && !(t == s_typeOfObject) && !(t == s_typeOfMonoBehavior))
		{
			BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			list.AddRange(t.GetProperties(bindingAttr));
			GetAllProperties(t.BaseType, ref list);
		}
	}

	private static void CacheVariablesForNulling(Type type)
	{
		VariableList variableList = new VariableList();
		GetAllFields(type, ref s_fieldInfo);
		foreach (FieldInfo item in s_fieldInfo)
		{
			if (item.FieldType.IsSubclassOf(s_typeOfObject) || item.FieldType.IsSubclassOf(s_typeOfMulticastDelegate) || item.FieldType.IsSubclassOf(s_typeOfSystemObject) || item.FieldType.IsSubclassOf(s_typeOfEventHandler))
			{
				variableList.variables.Add(item);
			}
			else if (item.FieldType.IsSubclassOf(s_typeOfList))
			{
				variableList.lists.Add(item);
			}
			else if (item.FieldType.IsSubclassOf(s_typeOfDictionary))
			{
				variableList.dictionarys.Add(item);
			}
		}
		s_variableTypes.Add(type, variableList);
		s_fieldInfo.Clear();
	}

	public static void NullOutObjectReferences(MonoBehaviour behavior)
	{
		VariableList value = null;
		Type type = behavior.GetType();
		if (!s_variableTypes.TryGetValue(type, out value))
		{
			CacheVariablesForNulling(type);
			value = s_variableTypes[type];
		}
		foreach (FieldInfo variable in value.variables)
		{
			variable.SetValue(behavior, null);
		}
		foreach (FieldInfo list in value.lists)
		{
			if (list.GetValue(behavior) is IList obj)
			{
				s_listClearMethod.Invoke(obj, null);
			}
		}
		foreach (FieldInfo dictionary in value.dictionarys)
		{
			if (dictionary.GetValue(behavior) is IDictionary obj2)
			{
				s_dictionaryClearMethod.Invoke(obj2, null);
			}
		}
	}

	public static T CopyComponent<T>(T original, GameObject destination) where T : Component
	{
		Type type = original.GetType();
		Component component = destination.AddComponent(type);
		GetAllFields(type, ref s_fieldInfo);
		foreach (FieldInfo item in s_fieldInfo)
		{
			if (!item.FieldType.IsSubclassOf(s_typeOfMulticastDelegate))
			{
				item.SetValue(component, item.GetValue(original));
			}
		}
		GetAllProperties(type, ref s_propertyInfo);
		foreach (PropertyInfo item2 in s_propertyInfo)
		{
			if (item2.GetGetMethod() != null && item2.GetSetMethod() != null)
			{
				item2.SetValue(component, item2.GetValue(original, null), null);
			}
		}
		s_fieldInfo.Clear();
		s_propertyInfo.Clear();
		return component as T;
	}

	public static T CopyScriptableObject<T>(T original) where T : ScriptableObject
	{
		T val = UnityEngine.Object.Instantiate(original);
		Type type = original.GetType();
		GetAllFields(type, ref s_fieldInfo);
		foreach (FieldInfo item in s_fieldInfo)
		{
			if (!item.FieldType.IsSubclassOf(s_typeOfMulticastDelegate))
			{
				item.SetValue(val, item.GetValue(original));
			}
		}
		GetAllProperties(type, ref s_propertyInfo);
		foreach (PropertyInfo item2 in s_propertyInfo)
		{
			if (item2.GetGetMethod() != null && item2.GetSetMethod() != null)
			{
				item2.SetValue(val, item2.GetValue(original, null), null);
			}
		}
		s_fieldInfo.Clear();
		s_propertyInfo.Clear();
		return val;
	}
}
