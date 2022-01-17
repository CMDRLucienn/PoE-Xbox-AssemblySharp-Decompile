using System;

[Serializable]
public class AreaNotificationsDatabaseString : DatabaseString
{
	public override bool IsStringTableMutable => false;

	public AreaNotificationsDatabaseString()
		: base(StringTableType.AreaNotifications)
	{
	}

	public AreaNotificationsDatabaseString(int id)
		: base(StringTableType.AreaNotifications, id)
	{
	}

	public override StringTableType GetStringTable()
	{
		return StringTableType.AreaNotifications;
	}
}
