using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using UnityEngine;

public class PrefabConverter<T> : TypeConverter where T : UnityEngine.Component
{
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		return destinationType == typeof(T);
	}

	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		return sourceType == typeof(T);
	}

	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		MonoBehaviour monoBehaviour = value as MonoBehaviour;
		if ((bool)monoBehaviour)
		{
			GameObject gameObject = monoBehaviour.gameObject;
			Persistence component = gameObject.GetComponent<Persistence>();
			if ((bool)component)
			{
				if (string.IsNullOrEmpty(component.Prefab))
				{
					Debug.LogWarning("The item " + gameObject.name + " doesn't have a prefab setup! It is going to be loaded from the asset bundle. THIS IS A BUG!");
					string text = gameObject.name;
					if (text.Contains("(Clone)"))
					{
						text = text.Substring(0, text.IndexOf('('));
					}
					return text;
				}
				return component.Prefab;
			}
			Debug.LogWarning(gameObject.name + " doesn't have a Persistence component. Save/Load will not work correctly for this object.", gameObject);
			return gameObject;
		}
		Debug.LogWarning(value.GetType().ToString() + " is marked for Prefab conversion but isn't a MonoBehaviour!?");
		return value;
	}

	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		T val = null;
		try
		{
			val = GameResources.LoadPrefab<T>(value.ToString(), Path.GetFileNameWithoutExtension(value.ToString()), instantiate: true);
			if ((UnityEngine.Object)val == (UnityEngine.Object)null)
			{
				val = GameResources.LoadPrefab<T>(Path.GetFileNameWithoutExtension(value.ToString()), instantiate: false);
			}
			if ((UnityEngine.Object)val != (UnityEngine.Object)null)
			{
				Persistence component = val.GetComponent<Persistence>();
				if ((bool)component)
				{
					component.IsPrefab = true;
				}
			}
		}
		finally
		{
			if ((UnityEngine.Object)val == (UnityEngine.Object)null)
			{
				Debug.LogError("There was an error during load. The prefab at " + value.ToString() + " isn't valid!");
			}
		}
		if ((UnityEngine.Object)val == (UnityEngine.Object)null)
		{
			return value;
		}
		return val.GetComponent<T>();
	}
}
