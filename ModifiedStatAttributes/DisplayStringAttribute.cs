using System;

namespace ModifiedStatAttributes;

[AttributeUsage(AttributeTargets.Field)]
public sealed class DisplayStringAttribute : Attribute
{
	public DatabaseString String;

	public DisplayStringAttribute(int id)
	{
		String = new DatabaseString(DatabaseString.StringTableType.Gui, id);
	}
}
