namespace Onyx;

public static class StringExtender
{
	public static bool IsNullOrWhitespace(string str)
	{
		if (string.IsNullOrEmpty(str))
		{
			return true;
		}
		for (int i = 0; i < str.Length; i++)
		{
			if (!char.IsWhiteSpace(str[i]))
			{
				return false;
			}
		}
		return true;
	}
}
