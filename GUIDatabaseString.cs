using System;

[Serializable]
public class GUIDatabaseString : DatabaseString
{
	public override bool IsStringTableMutable => false;

	public GUIDatabaseString()
		: base(StringTableType.Gui)
	{
	}

	public GUIDatabaseString(int stringId)
		: base(StringTableType.Gui, stringId)
	{
	}

	public override StringTableType GetStringTable()
	{
		return StringTableType.Gui;
	}
}
