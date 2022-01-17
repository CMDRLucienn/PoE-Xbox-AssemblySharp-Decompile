using System;

[Serializable]
public class BackstoryDatabaseString : DatabaseString
{
	public override bool IsStringTableMutable => false;

	public BackstoryDatabaseString()
		: base(StringTableType.Backstory)
	{
	}

	public BackstoryDatabaseString(int id)
		: base(StringTableType.Backstory, id)
	{
	}

	public override StringTableType GetStringTable()
	{
		return StringTableType.Backstory;
	}
}
