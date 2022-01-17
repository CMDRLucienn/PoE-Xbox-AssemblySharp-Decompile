using System;

[Serializable]
public class FactionDatabaseString : DatabaseString
{
	public override bool IsStringTableMutable => false;

	public FactionDatabaseString()
		: base(StringTableType.Factions)
	{
	}

	public FactionDatabaseString(int id)
		: base(StringTableType.Factions)
	{
		StringID = id;
	}

	public override StringTableType GetStringTable()
	{
		return StringTableType.Factions;
	}
}
