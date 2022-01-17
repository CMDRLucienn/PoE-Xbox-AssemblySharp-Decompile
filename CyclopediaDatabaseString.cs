using System;

[Serializable]
public class CyclopediaDatabaseString : DatabaseString
{
	public override bool IsStringTableMutable => false;

	public CyclopediaDatabaseString()
		: base(StringTableType.Cyclopedia)
	{
	}

	public CyclopediaDatabaseString(int stringId)
		: base(StringTableType.Cyclopedia, stringId)
	{
	}

	public override StringTableType GetStringTable()
	{
		return StringTableType.Cyclopedia;
	}
}
