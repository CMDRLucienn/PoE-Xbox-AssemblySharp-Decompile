using System.Globalization;

public static class UIntUtils
{
	public static bool TryParseInvariant(string str, out uint val)
	{
		return uint.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out val);
	}

	public static uint ParseInvariant(string str)
	{
		return uint.Parse(str, CultureInfo.InvariantCulture);
	}

	public static string ToStringInvariant(this uint val)
	{
		return val.ToString(CultureInfo.InvariantCulture);
	}
}
