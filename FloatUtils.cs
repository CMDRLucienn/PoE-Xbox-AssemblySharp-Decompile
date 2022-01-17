using System.Globalization;

public static class FloatUtils
{
	public static bool TryParseInvariant(string str, out float val)
	{
		return float.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out val);
	}

	public static float ParseInvariant(string str)
	{
		return float.Parse(str, CultureInfo.InvariantCulture);
	}

	public static string ToStringInvariant(this float val)
	{
		return val.ToString(CultureInfo.InvariantCulture);
	}
}
