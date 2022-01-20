// IEMod.Helpers.PlayerPrefsHelper
using System;
using System.IO;
using System.Xml.Serialization;
using Patchwork.Attributes;
using UnityEngine;

[PatchedByType("IEMod.Helpers.PlayerPrefsHelper")]
[NewType(null, null)]
public class PlayerPrefsHelper
{
	[PatchedByMember("System.Void IEMod.Helpers.PlayerPrefsHelper::SetBool(System.String,System.Boolean)")]
	public static void SetBool(string name, bool value)
	{
		PlayerPrefs.SetInt(name, value ? 1 : 0);
	}

	[PatchedByMember("System.Boolean IEMod.Helpers.PlayerPrefsHelper::GetBool(System.String,System.Boolean)")]
	public static bool GetBool(string name, bool defaultValue = false)
	{
		return PlayerPrefs.GetInt(name, defaultValue ? 1 : 0) == 1;
	}

	[PatchedByMember("System.Object IEMod.Helpers.PlayerPrefsHelper::GetXmlObject(System.String,System.Type)")]
	private static object GetXmlObject(string name, Type type)
	{
		if (!PlayerPrefs.HasKey(name))
		{
			return null;
		}
		XmlSerializer xmlSerializer = new XmlSerializer(type);
		string @string = PlayerPrefs.GetString(name);
		if (string.IsNullOrEmpty(@string))
		{
			return Activator.CreateInstance(type);
		}
		object result = null;
		try
		{
			result = xmlSerializer.Deserialize(new StringReader(@string));
		}
		catch (Exception)
		{
			IEDebug.Log("Error when deserializing: " + name);
		}
		return result;
	}

	[PatchedByMember("System.Void IEMod.Helpers.PlayerPrefsHelper::SetXmlObject(System.String,System.Type,System.Object)")]
	private static void SetXmlObject(string name, Type type, object o)
	{
		if (o == null)
		{
			PlayerPrefs.DeleteKey(name);
		}
		XmlSerializer xmlSerializer = new XmlSerializer(type);
		StringWriter stringWriter = new StringWriter();
		xmlSerializer.Serialize(stringWriter, o);
		string value = stringWriter.ToString();
		PlayerPrefs.SetString(name, value);
	}

	[PatchedByMember("System.Object IEMod.Helpers.PlayerPrefsHelper::GetObject(System.String,System.Type)")]
	public static object GetObject(string name, Type type)
	{
		Type type2 = (type.IsEnum ? Enum.GetUnderlyingType(type) : type);
		object obj;
		if (type2 == typeof(bool))
		{
			obj = GetBool(name);
		}
		else if (type2 == typeof(int))
		{
			obj = PlayerPrefs.GetInt(name, 0);
		}
		else if (type2 == typeof(string))
		{
			obj = PlayerPrefs.GetString(name, "");
		}
		else
		{
			if (!(type2 == typeof(float)))
			{
				IEDebug.Log("Going to try to deserialize PlayerPref {0} as XML", name);
				return GetXmlObject(name, type);
			}
			obj = PlayerPrefs.GetFloat(name, 0f);
		}
		return type.IsEnum ? Enum.ToObject(type, obj) : obj;
	}

	[PatchedByMember("System.Void IEMod.Helpers.PlayerPrefsHelper::SetObject(System.String,System.Type,System.Object)")]
	public static void SetObject(string name, Type type, object o)
	{
		Type type2 = (type.IsEnum ? Enum.GetUnderlyingType(type) : type);
		if (type2 == typeof(bool))
		{
			SetBool(name, (bool)o);
			return;
		}
		if (type2 == typeof(int))
		{
			PlayerPrefs.SetInt(name, (int)o);
			return;
		}
		if (type2 == typeof(string))
		{
			PlayerPrefs.SetString(name, (string)o);
			return;
		}
		if (type2 == typeof(float))
		{
			PlayerPrefs.SetFloat(name, (float)o);
			return;
		}
		IEDebug.Log("Going to try to serialize PlayerPref '{0}' as XML", name);
		SetXmlObject(name, type, o);
	}

	[PatchedByMember("System.Void IEMod.Helpers.PlayerPrefsHelper::.ctor()")]
	public PlayerPrefsHelper()
	{
	}
}
