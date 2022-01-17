using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using OEICommon;
using UnityEngine;

public static class StringTableManager
{
	public delegate void LanguageChanged(Language newLanguage);

	private class DescendingComparer<T> : IComparer<T> where T : IComparable<T>
	{
		public int Compare(T x, T y)
		{
			return y.CompareTo(x);
		}
	}

	public static bool StringDebug;

	private static bool Initialized;

	public static Language CurrentLanguage;

	public static Language DefaultLanguage;

	public static List<Language> Languages;

	private static Dictionary<string, SortedDictionary<uint, StringTable>> StringTables;

	private static Dictionary<DatabaseString.StringTableType, string> StringTableLookup;

	public static Gender PlayerGender;

	public static event LanguageChanged OnLanguageChanged;

	static StringTableManager()
	{
		StringDebug = false;
		Initialized = false;
		CurrentLanguage = null;
		DefaultLanguage = null;
		Languages = new List<Language>();
		StringTables = new Dictionary<string, SortedDictionary<uint, StringTable>>();
		StringTableLookup = new Dictionary<DatabaseString.StringTableType, string>();
		PlayerGender = Gender.Male;
	}

	public static void Cleanup()
	{
		StringTableManager.OnLanguageChanged = null;
	}

	public static void Init()
	{
		if (!Initialized)
		{
			Initialized = true;
			LoadLanguages();
			InitInGameStringTables();
			ReloadStringTables();
		}
	}

	private static void LoadLanguages()
	{
		string[] directories = Directory.GetDirectories(Application.dataPath + "/data/localized", "*", SearchOption.TopDirectoryOnly);
		for (int i = 0; i < directories.Length; i++)
		{
			string text = Path.Combine(directories[i], "language.xml").Replace("\\", "/");
			if (!File.Exists(text))
			{
				continue;
			}
			Language language = Language.Load(text);
			if (language.IsValid())
			{
				Languages.Add(language);
				if (string.Compare(language.Name, "english", ignoreCase: true) == 0)
				{
					DefaultLanguage = language;
				}
			}
		}
		CurrentLanguage = GetLanguage(Application.systemLanguage.ToString().ToLower());
		if (CurrentLanguage == null)
		{
			CurrentLanguage = DefaultLanguage;
		}
	}

	private static void InitInGameStringTables()
	{
		DatabaseString.StringTableType[] array = (DatabaseString.StringTableType[])Enum.GetValues(typeof(DatabaseString.StringTableType));
		for (int i = 0; i < array.Length; i++)
		{
			DatabaseString.StringTableType stringTableType = array[i];
			if (stringTableType != 0 && stringTableType != DatabaseString.StringTableType.Debug)
			{
				string text = "text/game/" + stringTableType.ToString().ToLower() + ".stringtable";
				StringTables.Add(text, new SortedDictionary<uint, StringTable>(new DescendingComparer<uint>()));
				StringTableLookup.Add(stringTableType, text);
			}
		}
	}

	public static void ReloadStringTables()
	{
		if (CurrentLanguage == null)
		{
			return;
		}
		ProductConfiguration.Package[] array = (ProductConfiguration.Package[])Enum.GetValues(typeof(ProductConfiguration.Package));
		foreach (string key in StringTables.Keys)
		{
			SortedDictionary<uint, StringTable> sortedDictionary = StringTables[key];
			sortedDictionary.Clear();
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] != ProductConfiguration.Package.BackerBeta)
				{
					string text = ProductConfiguration.PackageDataFolders[i] + "/localized/" + CurrentLanguage.Folder + "/" + key;
					text = Application.dataPath + Path.DirectorySeparatorChar + text.ToLower();
					if (!File.Exists(text) && DefaultLanguage != null)
					{
						text = ProductConfiguration.PackageDataFolders[i] + "/localized/" + DefaultLanguage.Folder + "/" + key;
						text = Application.dataPath + Path.DirectorySeparatorChar + text.ToLower();
					}
					sortedDictionary.Add((uint)array[i], StringTable.Load(text, (uint)array[i]));
				}
			}
		}
	}

	public static void LoadStringTable(string relativePath)
	{
		if (StringTables.ContainsKey(relativePath))
		{
			return;
		}
		SortedDictionary<uint, StringTable> sortedDictionary = new SortedDictionary<uint, StringTable>(new DescendingComparer<uint>());
		StringTables.Add(relativePath, sortedDictionary);
		ProductConfiguration.Package[] array = (ProductConfiguration.Package[])Enum.GetValues(typeof(ProductConfiguration.Package));
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != ProductConfiguration.Package.BackerBeta)
			{
				string filename = ProductConfiguration.PackageDataFolders[i] + "/localized/" + CurrentLanguage.Folder + "/" + relativePath;
				filename = GameResources.GetOverridePath(Path.GetDirectoryName("/localized/" + CurrentLanguage.Folder + "/" + relativePath), filename);
				filename = Application.dataPath + Path.DirectorySeparatorChar + filename.ToLower();
				if (!File.Exists(filename) && DefaultLanguage != null)
				{
					filename = ProductConfiguration.PackageDataFolders[i] + "/localized/" + DefaultLanguage.Folder + "/" + relativePath;
					filename = Application.dataPath + Path.DirectorySeparatorChar + filename.ToLower();
				}
				sortedDictionary.Add((uint)array[i], StringTable.Load(filename, (uint)array[i]));
			}
		}
	}

	public static void UnloadStringTable(string relativePath)
	{
		if (StringTables.ContainsKey(relativePath))
		{
			StringTables.Remove(relativePath);
		}
	}

	public static string GetText(DatabaseString.StringTableType stringTable, int stringID)
	{
		return GetText(stringTable, stringID, PlayerGender);
	}

	public static string GetText(DatabaseString.StringTableType stringTable, int stringID, Gender gender)
	{
		if (!Initialized)
		{
			return string.Empty;
		}
		if (StringTableLookup.ContainsKey(stringTable))
		{
			return GetText(StringTableLookup[stringTable], stringID, gender);
		}
		if (stringID >= 0)
		{
			Debug.Log($"Could not find string for Table {stringTable.ToString()} ID {stringID}");
		}
		return "*Missing " + stringTable.ToString() + " " + stringID + "*";
	}

	public static string GetText(string stringTableFilename, int stringID)
	{
		return GetText(stringTableFilename, stringID, PlayerGender);
	}

	public static string GetCharacterName(DatabaseString databaseString, Gender characterGender)
	{
		return GetText(StringTableLookup[DatabaseString.StringTableType.Characters], databaseString.StringID, characterGender);
	}

	private static string GetText(string stringTableFilename, int stringID, Gender gender)
	{
		if (!Initialized)
		{
			return string.Empty;
		}
		if (StringTables.ContainsKey(stringTableFilename))
		{
			SortedDictionary<uint, StringTable> sortedDictionary = StringTables[stringTableFilename];
			foreach (uint key in sortedDictionary.Keys)
			{
				if (!sortedDictionary.ContainsKey(key))
				{
					continue;
				}
				StringTable.Entry entry = sortedDictionary[key].Entries.Find((StringTable.Entry s) => s.StringID == stringID);
				if (entry != null)
				{
					string text = ((gender == Gender.Female && !string.IsNullOrEmpty(entry.FemaleText)) ? entry.FemaleText : entry.DefaultText);
					if (StringDebug)
					{
						text = text + " <" + Path.GetFileNameWithoutExtension(stringTableFilename) + ": " + stringID + ">";
					}
					return text;
				}
			}
		}
		if (stringID == 0)
		{
			return string.Empty;
		}
		if (stringID > 0)
		{
			Debug.Log($"Could not find string for Table {stringTableFilename.ToString()} ID {stringID}");
		}
		return "*Missing " + Path.GetFileNameWithoutExtension(stringTableFilename) + " " + stringID + "*";
	}

	public static bool FemaleVersionExists(string stringTableFilename, int stringID)
	{
		if (!Initialized)
		{
			return false;
		}
		if (StringTables.ContainsKey(stringTableFilename))
		{
			SortedDictionary<uint, StringTable> sortedDictionary = StringTables[stringTableFilename];
			foreach (uint key in sortedDictionary.Keys)
			{
				if (sortedDictionary.ContainsKey(key))
				{
					StringTable.Entry entry = sortedDictionary[key].Entries.Find((StringTable.Entry s) => s.StringID == stringID);
					if (entry != null && !string.IsNullOrEmpty(entry.FemaleText))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public static Language GetLanguage(string name)
	{
		IEnumerable<Language> source = Languages.Where((Language lang) => lang.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
		if (source.Any())
		{
			return source.First();
		}
		return null;
	}

	public static void SetCurrentLanguageByName(string name)
	{
		SetCurrentLanguage(GetLanguage(name));
	}

	public static void SetCurrentLanguage(Language newCurrentLanguage)
	{
		if (newCurrentLanguage != null && newCurrentLanguage != CurrentLanguage)
		{
			CurrentLanguage = newCurrentLanguage;
			try
			{
				Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(OEICommon.Localization.GetLanguageCode(CurrentLanguage.EnumLanguage));
			}
			catch (Exception)
			{
				Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			}
			ReloadStringTables();
			if (StringTableManager.OnLanguageChanged != null)
			{
				StringTableManager.OnLanguageChanged(CurrentLanguage);
			}
		}
	}
}
