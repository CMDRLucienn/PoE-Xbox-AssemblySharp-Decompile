using System;

[Serializable]
public class InteractablesDatabaseString : DatabaseString
{
	public override bool IsStringTableMutable => false;

	public InteractablesDatabaseString()
		: base(StringTableType.Interactables)
	{
	}

	public InteractablesDatabaseString(int id)
		: base(StringTableType.Interactables, id)
	{
	}

	public override StringTableType GetStringTable()
	{
		return StringTableType.Interactables;
	}
}
