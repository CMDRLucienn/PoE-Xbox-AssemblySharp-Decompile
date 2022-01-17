using System;

[Serializable]
public class MapsDatabaseString : DatabaseString
{
	public override bool IsStringTableMutable => false;

	public MapsDatabaseString()
		: base(StringTableType.Maps)
	{
	}

	public MapsDatabaseString(int id)
		: base(StringTableType.Maps, id)
	{
	}

	public override StringTableType GetStringTable()
	{
		return StringTableType.Maps;
	}
}
