using System;

public static class StrongholdUtils
{
	public static string GetText(int id)
	{
		return StringTableManager.GetText(DatabaseString.StringTableType.Stronghold, id);
	}

	public static string GetText(int id, Gender gender)
	{
		return StringTableManager.GetText(DatabaseString.StringTableType.Stronghold, id, gender);
	}

	public static string Format(int id, params object[] parameters)
	{
		try
		{
			return string.Format(GetText(id), parameters);
		}
		catch (FormatException)
		{
			return "";
		}
	}

	public static string Format(Gender gender, int id, params object[] parameters)
	{
		try
		{
			return string.Format(GetText(id, gender), parameters);
		}
		catch (FormatException)
		{
			return "";
		}
	}
}
