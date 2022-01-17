using System;

[Serializable]
public class CharacterDatabaseString : DatabaseString
{
	public string CharacterGuid;

	public string CharacterGuidSerialized
	{
		get
		{
			return CharacterGuid;
		}
		set
		{
			CharacterGuid = value;
		}
	}

	public override bool IsStringTableMutable => false;

	public CharacterDatabaseString() : base(StringTableType.Characters)
	{
		Guid empty = Guid.Empty;
		CharacterGuid = empty.ToString();
	}

	public override StringTableType GetStringTable()
	{
		return StringTableType.Characters;
	}
}
