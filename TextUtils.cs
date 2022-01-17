using System;
using System.Collections.Generic;
using System.Text;

public static class TextUtils
{
	private static StringBuilder s_StringBuilder = new StringBuilder();

	public static string IndexToAlphabet(int index)
	{
		string text = "";
		do
		{
			int num = index % 26;
			index /= 26;
			text += (char)(65 + num);
		}
		while (index > 0);
		return text;
	}

	public static string ReplaceEncodingSafe(this string str, char replace, char with)
	{
		s_StringBuilder.Remove(0, s_StringBuilder.Length);
		s_StringBuilder.Append(str);
		bool flag = false;
		for (int num = str.Length - 1; num >= 0; num--)
		{
			if (str[num] == ']')
			{
				flag = true;
			}
			else if (str[num] == '[')
			{
				flag = false;
			}
			else if (!flag && str[num] == replace)
			{
				s_StringBuilder.Replace(replace, with, num, 1);
			}
		}
		return s_StringBuilder.ToString();
	}

	public static string ReplaceEncodingSafe(this string str, string replace, string with)
	{
		bool flag = false;
		int num = replace.Length - 1;
		for (int num2 = str.Length - 1; num2 >= 0; num2--)
		{
			if (str[num2] == ']')
			{
				flag = true;
			}
			else if (str[num2] == '[')
			{
				flag = false;
			}
			else if (!flag && str[num2] == replace[num])
			{
				if (num == 0)
				{
					num = replace.Length - 1;
					str = str.Remove(num2, replace.Length);
					str = str.Insert(num2, with);
				}
				else
				{
					num--;
				}
			}
			else
			{
				num = replace.Length - 1;
			}
		}
		return str;
	}

	public static string CapitalizeFirst(this string str)
	{
		if (str.Length > 0 && str[0] >= 'a' && str[0] <= 'z')
		{
			return (char)(str[0] + -32) + str.Substring(1);
		}
		return str;
	}

	public static string FuncJoin<T>(Func<T, string> conversion, IEnumerable<T> items, string seperator)
	{
		return FuncJoin(conversion, items, seperator, removeEmpty: false);
	}

	public static string FuncJoin<T>(Func<T, string> conversion, IEnumerable<T> items, string seperator, bool removeEmpty)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (T item in items)
		{
			string value = conversion(item);
			if (!removeEmpty || !string.IsNullOrEmpty(value))
			{
				stringBuilder.Append(value);
				stringBuilder.Append(seperator);
			}
		}
		if (stringBuilder.Length > 0)
		{
			stringBuilder.Remove(stringBuilder.Length - seperator.Length, seperator.Length);
		}
		return stringBuilder.ToString();
	}

	public static string Join(IEnumerable<string> items, string seperator, bool removeEmpty)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (string item in items)
		{
			if (!removeEmpty || !string.IsNullOrEmpty(item))
			{
				stringBuilder.Append(item);
				stringBuilder.Append(seperator);
			}
		}
		if (stringBuilder.Length > 0)
		{
			stringBuilder.Remove(stringBuilder.Length - seperator.Length, seperator.Length);
		}
		return stringBuilder.ToString();
	}

	public static string NumberBonus(int value)
	{
		if (value >= 0)
		{
			return "+" + value;
		}
		return value.ToString();
	}

	public static string NumberBonus(int value, string format)
	{
		if (value >= 0)
		{
			return "+" + value.ToString(format);
		}
		return value.ToString(format);
	}

	public static string NumberBonus(float value)
	{
		if (value >= 0f)
		{
			return "+" + value;
		}
		return value.ToString();
	}

	public static string NumberBonus(float value, string format)
	{
		if (value >= 0f)
		{
			return "+" + value.ToString(format);
		}
		return value.ToString(format);
	}

	public static string MultiplierAsPercentBonus(float mult)
	{
		mult = ((mult == 0.33f) ? (-66f) : ((mult != 0.66f) ? ((mult - 1f) * 100f) : (-33f)));
		return GUIUtils.Format(1277, NumberBonus(mult, "#0"));
	}

	public static string FormatBase(float appliedValue, float adjustedValue, string format)
	{
		if (adjustedValue != appliedValue)
		{
			return AttackBase.FormatBase(adjustedValue.ToString(format), GUIUtils.Format(1555, appliedValue.ToString(format)), Math.Abs(appliedValue) < Math.Abs(adjustedValue));
		}
		return adjustedValue.ToString(format);
	}

	public static string FormatBase(float appliedValue, float adjustedValue, Func<float, string> format)
	{
		if (adjustedValue != appliedValue)
		{
			return AttackBase.FormatBase(format(adjustedValue), GUIUtils.Format(1555, format(appliedValue)), Math.Abs(appliedValue) < Math.Abs(adjustedValue));
		}
		return format(adjustedValue);
	}

	public static string LiteEscapeUrlForXml(string url)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < url.Length; i++)
		{
			if (url[i] == '\\')
			{
				stringBuilder.Append('/');
			}
			else if (url[i] == '%')
			{
				stringBuilder.Append("%25");
			}
			else
			{
				stringBuilder.Append(url[i]);
			}
		}
		return stringBuilder.ToString();
	}

	public static string LiteEscapeUrl(string url)
	{
		return LiteEscapeUrlForXml(url);
	}

	public static string Plural(string singular, string plural, int quantity)
	{
		if (quantity == 1)
		{
			return quantity + " " + singular;
		}
		return quantity + " " + plural;
	}
}
