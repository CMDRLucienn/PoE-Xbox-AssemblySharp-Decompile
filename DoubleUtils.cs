using System.Globalization;

public static class DoubleUtils
{
	public static bool TryParseInvariant(string str, out double val)
	{
		return double.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out val);
	}

	public static double ParseInvariant(string str)
	{
		return double.Parse(str, CultureInfo.InvariantCulture);
	}

	public static string ToStringInvariant(this double val)
	{
		return val.ToString(CultureInfo.InvariantCulture);
	}
}
