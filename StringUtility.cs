using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class StringUtility
{
	public static StringBuilder TrimEnd(this StringBuilder sb)
	{
		for (int num = sb.Length - 1; num >= 0; num--)
		{
			if (!char.IsWhiteSpace(sb[num]))
			{
				sb.Remove(num + 1, sb.Length - (num + 1));
				return sb;
			}
		}
		return sb;
	}

	public static StringBuilder Remove(this StringBuilder sb, int start)
	{
		return sb.Remove(start, sb.Length - start);
	}

	public static StringBuilder AppendGuiFormat(this StringBuilder sb, int stringId, params object[] param)
	{
		return sb.Append(GUIUtils.Format(stringId, param));
	}

	public static StringBuilder AppendCatchFormat(this StringBuilder sb, string fstring, params object[] param)
	{
		return sb.Append(Format(fstring, param));
	}

	public static StringBuilder AppendCatchFormatLine(this StringBuilder sb, string fstring, params object[] param)
	{
		return sb.AppendLine(Format(fstring, param));
	}

	public static string Format(DatabaseString fstring, params object[] param)
	{
		return Format(fstring.GetText(), param);
	}

	public static string Format(string fstring, params object[] param)
	{
		try
		{
			return string.Format(fstring, param);
		}
		catch (FormatException exception)
		{
			Debug.LogException(exception);
			return "";
		}
	}

	public static string SpaceWords(string input)
	{
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = false;
		for (int i = 0; i < input.Length; i++)
		{
			if (IsCapitalLetter(input[i]) && flag)
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append(input[i]);
			flag = IsLowercaseLetter(input[i]);
		}
		return stringBuilder.ToString();
	}

	public static bool IsCapitalLetter(char c)
	{
		if (c >= 'A')
		{
			return c <= 'Z';
		}
		return false;
	}

	public static bool IsLowercaseLetter(char c)
	{
		if (c >= 'a')
		{
			return c <= 'z';
		}
		return false;
	}

	public static char ToLower(char c)
	{
		if (IsCapitalLetter(c))
		{
			return (char)(c - 65 + 97);
		}
		return c;
	}

	public static char ToUpper(char c)
	{
		if (IsLowercaseLetter(c))
		{
			return (char)(c - 97 + 65);
		}
		return c;
	}

	public static string FormatWithColor(string fstring, Color stringcolor, params object[] pms)
	{
		StringBuilder stringBuilder = new StringBuilder();
		string colorstring = "[" + NGUITools.EncodeColor(stringcolor) + "]";
		stringBuilder.Append(colorstring);
		try
		{
			object[] param = pms.Select((object obj) => "[-]" + obj.ToString() + colorstring).ToArray();
			stringBuilder.AppendCatchFormat(fstring, param);
		}
		catch (FormatException message)
		{
			Debug.LogError(message);
			stringBuilder.Append("[-]*FormatException*" + colorstring);
		}
		stringBuilder.Append("[-]");
		return stringBuilder.ToString();
	}

	public static List<string> CommandLineStyleSplit(string input)
	{
		List<string> list = new List<string>();
		int num = 0;
		bool flag = false;
		for (int i = 0; i < input.Length; i++)
		{
			if ((!flag && char.IsWhiteSpace(input[i])) || (flag && input[i] == '"'))
			{
				string text = input.Substring(num, i - num).Trim();
				if (!string.IsNullOrEmpty(text))
				{
					list.Add(text);
				}
				num = i + 1;
				flag = false;
				continue;
			}
			if (i == input.Length - 1)
			{
				string text2 = input.Substring(num, i - num + 1).Trim();
				if (!string.IsNullOrEmpty(text2))
				{
					list.Add(text2);
				}
				break;
			}
			if (!flag && input[i] == '"')
			{
				flag = true;
				num = i + 1;
			}
		}
		return list;
	}
}
