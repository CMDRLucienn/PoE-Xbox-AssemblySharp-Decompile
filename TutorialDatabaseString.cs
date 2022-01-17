using System;

[Serializable]
public class TutorialDatabaseString : DatabaseString
{
	public override bool IsStringTableMutable => false;

	public TutorialDatabaseString()
		: base(StringTableType.Tutorial)
	{
	}

	public TutorialDatabaseString(int id)
		: base(StringTableType.Tutorial, id)
	{
	}

	public override StringTableType GetStringTable()
	{
		return StringTableType.Tutorial;
	}
}
