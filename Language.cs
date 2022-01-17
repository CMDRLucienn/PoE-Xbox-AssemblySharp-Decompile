using System;
using System.IO;
using System.Xml;
using OEICommon;
using UnityEngine;

public class Language
{
	public enum CharacterSet
	{
		Latin,
		Cyrillic,
		Hangul
	}

	public string Name { get; private set; }

	public string GUIString { get; private set; }

	public string Folder { get; private set; }

	public CharacterSet Charset { get; private set; }

	public OEICommon.Language EnumLanguage
	{
		get
		{
			try
			{
				return (OEICommon.Language)Enum.Parse(typeof(OEICommon.Language), Name, ignoreCase: true);
			}
			catch (ArgumentException)
			{
				return OEICommon.Language.english;
			}
		}
	}

	private Language()
	{
		Name = string.Empty;
		GUIString = string.Empty;
		Folder = string.Empty;
		Charset = CharacterSet.Latin;
	}

	public bool IsValid()
	{
		if (!string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(GUIString))
		{
			return !string.IsNullOrEmpty(Folder);
		}
		return false;
	}

	public static Language Load(string filename)
	{
		Language language = new Language();
		XmlDocument xmlDocument = new XmlDocument();
		try
		{
			xmlDocument.Load(TextUtils.LiteEscapeUrlForXml(filename));
		}
		catch (Exception ex)
		{
			Debug.Log("Failed to load " + filename + ":\n" + ex);
			return language;
		}
		XmlElement documentElement = xmlDocument.DocumentElement;
		if (documentElement == null)
		{
			return language;
		}
		foreach (XmlNode childNode in documentElement.ChildNodes)
		{
			if (childNode.Name == "Name")
			{
				language.Name = childNode.InnerText;
			}
			else if (childNode.Name == "GUIString")
			{
				language.GUIString = childNode.InnerText;
			}
			else if (childNode.Name == "Charset")
			{
				try
				{
					language.Charset = (CharacterSet)Enum.Parse(typeof(CharacterSet), childNode.InnerText, ignoreCase: true);
				}
				catch (ArgumentException)
				{
					language.Charset = CharacterSet.Latin;
				}
				catch (OverflowException)
				{
					language.Charset = CharacterSet.Latin;
				}
			}
		}
		DirectoryInfo directoryInfo = new DirectoryInfo(Path.GetDirectoryName(filename));
		language.Folder = directoryInfo.Name;
		return language;
	}

	public override string ToString()
	{
		return GUIString;
	}
}
