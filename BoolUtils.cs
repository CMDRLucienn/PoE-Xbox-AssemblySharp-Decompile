using System.Globalization;

public static class BoolUtils
{
	public static bool TryParseInvariant(string str, out bool val)
	{
		return bool.TryParse(str, out val);
	}

	public static bool ParseInvariant(string str)
	{
		return bool.Parse(str);
	}

	public static string ToStringInvariant(this bool val)
	{
		return val.ToString(CultureInfo.InvariantCulture);
	}
}
