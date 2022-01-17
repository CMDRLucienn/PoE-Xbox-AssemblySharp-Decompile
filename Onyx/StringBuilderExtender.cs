using System.Text;

namespace Onyx;

public static class StringBuilderExtender
{
	public static StringBuilder Clear(this StringBuilder builder)
	{
		builder.Length = 0;
		return builder;
	}

	public static bool IsNullOrWhitespace(StringBuilder str)
	{
		if (str == null || str.Length == 0)
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
