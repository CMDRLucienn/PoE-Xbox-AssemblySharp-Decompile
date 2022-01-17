using System;

[Serializable]
public class ItemsDatabaseString : DatabaseString
{
	public override bool IsStringTableMutable => false;

	public ItemsDatabaseString()
		: base(StringTableType.Items)
	{
	}

	public ItemsDatabaseString(int id)
		: base(StringTableType.Items, id)
	{
	}

	public override StringTableType GetStringTable()
	{
		return StringTableType.Items;
	}
}
