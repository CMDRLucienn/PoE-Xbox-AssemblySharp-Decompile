using System.Globalization;

public static class IntUtils
{
	public static bool TryParseInvariant(string str, out int val)
	{
		return int.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out val);
	}

	public static int ParseInvariant(string str)
	{
		return int.Parse(str, CultureInfo.InvariantCulture);
	}

	public static string ToStringInvariant(this int val)
	{
		return val.ToString(CultureInfo.InvariantCulture);
	}
}
