using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

public class StringTable
{
	public class Entry
	{
		public int StringID = -1;

		public string DefaultText = string.Empty;

		public string FemaleText = string.Empty;

		public uint Package;

		public Entry()
		{
		}

		public Entry(XmlNode parentNode)
		{
			foreach (XmlNode childNode in parentNode.ChildNodes)
			{
				if (childNode.Name == "ID")
				{
					int val = -1;
					if (IntUtils.TryParseInvariant(childNode.InnerText, out val))
					{
						StringID = val;
					}
				}
				else if (childNode.Name == "DefaultText")
				{
					DefaultText = childNode.InnerText;
				}
				else if (childNode.Name == "FemaleText")
				{
					FemaleText = childNode.InnerText;
				}
			}
		}
	}

	public List<Entry> Entries = new List<Entry>();

	public string Filename { get; private set; }

	public int Count => Entries.Count;

	public static StringTable Load(string filename, uint package)
	{
		StringTable stringTable = new StringTable();
		stringTable.Filename = filename.Substring(filename.IndexOf("/text/") + 1);
		if (!File.Exists(filename))
		{
			return stringTable;
		}
		XmlDocument xmlDocument = new XmlDocument();
		try
		{
			xmlDocument.Load(TextUtils.LiteEscapeUrlForXml(filename));
		}
		catch
		{
			Debug.Log("Failed to load " + filename + ".");
			return stringTable;
		}
		XmlElement documentElement = xmlDocument.DocumentElement;
		if (documentElement == null)
		{
			return stringTable;
		}
		foreach (XmlNode childNode in documentElement.ChildNodes)
		{
			if (childNode.Name == "EntryCount" && stringTable.Entries.Count == 0)
			{
				int val = 0;
				if (IntUtils.TryParseInvariant(childNode.InnerText, out val) && val > 0)
				{
					stringTable.Entries = new List<Entry>(val);
				}
			}
			else
			{
				if (!(childNode.Name == "Entries"))
				{
					continue;
				}
				foreach (XmlNode childNode2 in childNode.ChildNodes)
				{
					if (childNode2.Name == "Entry")
					{
						Entry entry = new Entry(childNode2);
						entry.Package = package;
						stringTable.Entries.Add(entry);
					}
				}
			}
		}
		return stringTable;
	}
}
