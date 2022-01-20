using System;
using UnityEngine;

[Serializable]
public class DatabaseString
{
	public enum StringTableType
	{
		/// <summary>
		/// We add an extra member to the enum for IEMod strings.
		/// That way they won't conflict with other GUI strings, and also lets us circumvent the standard string table system
		/// which is a massive pain in the underside.
		/// </summary>
		IEModGUI = -2135132,
		Unassigned = 0,
		Gui = 1,
		Characters = 4,
		Items = 5,
		Abilities = 6,
		AreaNotifications = 9,
		Interactables = 10,
		Debug = 12,
		Recipes = 13,
		Factions = 14,
		LoadingTips = 15,
		ItemMods = 16,
		Maps = 17,
		Afflictions = 19,
		Cyclopedia = 900,
		Stronghold = 938,
		Backstory = 942,
		BackerContent = 20,
		Tutorial = 7
	}

	public StringTableType StringTable;

	public int StringID;

	public uint Package;

	public int StringTableID
	{
		get
		{
			return (int)GetStringTable();
		}
		set
		{
			if (Enum.IsDefined(typeof(StringTableType), value))
			{
				StringTable = (StringTableType)value;
			}
			else
			{
				Debug.LogError("Attempting to assign an invalid string table ID.");
			}
		}
	}

	public virtual bool IsStringTableMutable => true;

	public bool IsValidString => StringID >= 0;

	public StringTableType StringTableSerialized
	{
		get
		{
			return StringTable;
		}
		set
		{
			StringTable = value;
		}
	}

	public int StringIDSerialized
	{
		get
		{
			return StringID;
		}
		set
		{
			StringID = value;
		}
	}

	public uint PackageSerialized
	{
		get
		{
			return Package;
		}
		set
		{
			Package = value;
		}
	}

	public DatabaseString()
	{
	}

	public DatabaseString(StringTableType stringTable)
		: this(stringTable, -1)
	{
	}

	public DatabaseString(StringTableType stringTable, int stringId)
	{
		StringTable = stringTable;
		StringID = stringId;
		Package = 0u;
	}

	public virtual string GetText()
	{
		return StringTableManager.GetText(GetStringTable(), StringID);
	}

	public string GetText(Gender gender)
	{
		return StringTableManager.GetText(GetStringTable(), StringID, gender);
	}

	public string GetTextWithLinks()
	{
		string text = GetText();
		if (GetStringTable() != StringTableType.Characters && GetStringTable() != StringTableType.Items && Glossary.Instance != null)
		{
			text = Glossary.Instance.AddUrlTags(text);
		}
		return text;
	}

	public virtual StringTableType GetStringTable()
	{
		return StringTable;
	}

	public override string ToString()
	{
		return GetText();
	}

	public override bool Equals(object obj)
	{
		if (obj is DatabaseString)
		{
			if (GetStringTable().Equals(((DatabaseString)obj).GetStringTable()))
			{
				return StringID.Equals(((DatabaseString)obj).StringID);
			}
			return false;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return GetText().GetHashCode();
	}
}
