using System;

[Serializable]
public class StrongholdDatabaseString : DatabaseString
{
	public override bool IsStringTableMutable => false;

	public StrongholdDatabaseString()
		: base(StringTableType.Stronghold)
	{
	}

	public StrongholdDatabaseString(int id)
		: base(StringTableType.Stronghold, id)
	{
	}

	public override StringTableType GetStringTable()
	{
		return StringTableType.Stronghold;
	}
}
