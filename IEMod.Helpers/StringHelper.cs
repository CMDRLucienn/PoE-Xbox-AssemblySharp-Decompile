// IEMod.Helpers.StringHelper
using System.Collections.Generic;
using System.Linq;
using Patchwork.Attributes;

[NewType(null, null)]
[PatchedByType("IEMod.Helpers.StringHelper")]
public static class StringHelper
{
	[PatchedByMember("System.String IEMod.Helpers.StringHelper::ReplaceAll(System.String,System.String,System.String[])")]
	public static string ReplaceAll(this string str, string replaceWith, params string[] replaceWhat)
	{
		return replaceWhat.Aggregate(str, (string current, string what) => current.Replace(what, replaceWith));
	}

	[PatchedByMember("System.String IEMod.Helpers.StringHelper::SentenceCase(System.String)")]
	public static string SentenceCase(this string str)
	{
		if (string.IsNullOrEmpty(str))
		{
			return str;
		}
		return char.ToUpper(str[0]) + str.Substring(1);
	}

	[PatchedByMember("System.String IEMod.Helpers.StringHelper::Join(System.Collections.Generic.IEnumerable`1<System.String>,System.String)")]
	public static string Join(this IEnumerable<string> str, string separator)
	{
		return string.Join(separator, str.ToArray());
	}
}
