using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public class Glossary : MonoBehaviour
{
	public enum GlossaryCategory
	{
		CharacterStatistics,
		CombatMechanics,
		AfflictionsAndInjuries,
		Equipment,
		Deities,
		Locations,
		Lore,
		Tips
	}

	public GlossaryEntryList glossaryEntries;

	private Dictionary<string, GlossaryEntry> entries = new Dictionary<string, GlossaryEntry>();

	private Dictionary<string, string> invalidPrefixes = new Dictionary<string, string>();

	private Dictionary<string, string> invalidSuffixes = new Dictionary<string, string>();

	private bool m_Initialized;

	public bool LinkAllWords;

	public bool LinkGlossaryWords;

	public const string GLOSSARY_MARKER_START = "[g]";

	public const string GLOSSARY_MARKER_END = "[/g]";

	public const string URL_PREFIX = "glossary";

	public static Glossary Instance { get; private set; }

	public bool Initialized => m_Initialized;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Debug.LogError("Singleton component 'Glossary' awoke multiple times! Remove this component from all objects except global prefabs. (Second object: '" + base.name + "')");
		}
		BuildEntries();
		StringTableManager.OnLanguageChanged += OnLanguageChanged;
		m_Initialized = true;
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		StringTableManager.OnLanguageChanged -= OnLanguageChanged;
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void OnLanguageChanged(Language lang)
	{
		BuildEntries();
	}

	private void BuildEntries()
	{
		invalidSuffixes.Clear();
		invalidPrefixes.Clear();
		entries.Clear();
		foreach (GlossaryEntry item in from e in glossaryEntries.Entries
			where e
			select e into x
			orderby x.Title.ToString().Length descending
			select x)
		{
			if (!item.Title.IsValidString)
			{
				continue;
			}
			string text = item.Title.GetText();
			string text2 = null;
			foreach (KeyValuePair<string, GlossaryEntry> entry in entries)
			{
				bool flag = !item.CaseSensitive && !entry.Value.CaseSensitive;
				if (entry.Key.Equals(text, flag ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture))
				{
					text2 = entry.Key;
					break;
				}
			}
			if (text2 != null)
			{
				if (!item.IsRedirect)
				{
					if (entries[text2].IsRedirect)
					{
						entries[text2] = item;
					}
					else
					{
						UIDebug.Instance.LogOnScreenWarning("Duplicate glossary entry: " + text + ".", UIDebug.Department.Design, 10f);
					}
				}
			}
			else
			{
				BuildInvalidPrefixesAndSuffixes(text);
				entries.Add(text, item);
			}
		}
	}

	public GlossaryEntry GetEntryByName(string name, bool allowRedirect)
	{
		GlossaryEntry glossaryEntry = null;
		if (entries.ContainsKey(name))
		{
			glossaryEntry = entries[name];
		}
		else if (entries.ContainsKey(name.ToLower()))
		{
			glossaryEntry = entries[name.ToLower()];
		}
		if (glossaryEntry != null && allowRedirect && glossaryEntry.IsRedirect)
		{
			if (glossaryEntry.LinkedEntries.Length != 0)
			{
				glossaryEntry = glossaryEntry.LinkedEntries[0];
			}
			else
			{
				UIDebug.Instance.LogOnScreenWarning("Glossary entry '" + glossaryEntry.name + "' is a redirect but it has no Linked Entries.", UIDebug.Department.Design, 10f);
			}
		}
		return glossaryEntry;
	}

	public string AddUrlTags(string text)
	{
		return AddUrlTags(text, null, carefulReplace: false);
	}

	public string AddUrlTags(string text, bool carefulReplace)
	{
		return AddUrlTags(text, null, carefulReplace);
	}

	public string AddUrlTags(string text, GlossaryEntry termToIgnore, bool carefulReplace)
	{
		text = StripGlossaryUrlTags(text);
		if (carefulReplace)
		{
			StringBuilder stringBuilder = new StringBuilder();
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			while (num2 < text.Length)
			{
				num2 = text.IndexOf("[g]", num2, StringComparison.OrdinalIgnoreCase);
				if (num2 < 0 || num2 >= text.Length)
				{
					stringBuilder.Append(text.Substring(num, text.Length - 1 - num));
					break;
				}
				stringBuilder.Append(text.Substring(num, num2 - num));
				num2 += "[g]".Length;
				num3 = text.IndexOf("[/g]", num2, StringComparison.OrdinalIgnoreCase);
				if (num3 > text.Length || num3 < 0)
				{
					num3 = text.Length - 1;
				}
				string text2 = text.Substring(num2, num3 - num2);
				string value = AddUrlTags(text2, termToIgnore, carefulReplace: false);
				stringBuilder.Append(value);
				num = Mathf.Min(num3 + "[/g]".Length, text.Length - 1);
			}
			return stringBuilder.ToString();
		}
		StringBuilder stringBuilder2 = new StringBuilder();
		foreach (string key in entries.Keys)
		{
			if (!string.IsNullOrEmpty(key) && entries[key].IsVisible && text.IndexOf(key, StringComparison.OrdinalIgnoreCase) >= 0 && (termToIgnore == null || !IsEntryOrRedirectToEntry(entries[key], termToIgnore)))
			{
				string text3 = Regex.Escape(entries[key].CaseSensitive ? key : key.ToLower());
				stringBuilder2.Append(entries[key].CaseSensitive ? "(?:)" : "(?i)");
				stringBuilder2.Append("(\\b");
				stringBuilder2.Append(text3);
				if (!text3.EndsWith("."))
				{
					stringBuilder2.Append("\\b");
				}
				stringBuilder2.Append(")");
				string pattern = string.Concat(invalidPrefixes[key], stringBuilder2, invalidSuffixes[key]);
				stringBuilder2.Length = 0;
				text = Regex.Replace(text, pattern, "[url=glossary://" + key + "]$1[/url]");
			}
		}
		int num4 = 0;
		int num5 = 0;
		for (int i = 0; i < text.Length; i++)
		{
			if (text[i] == '[')
			{
				if (num4 == 0)
				{
					num5 = i;
				}
				num4++;
			}
			else if (text[i] == ']')
			{
				num4--;
				if (num4 <= 0)
				{
					num4 = 0;
					int num6 = num5 + 1;
					int num7 = i - 1;
					int length = text.Length;
					text = text.Substring(0, num6) + StripGlossaryUrlTags(text.Substring(num6, num7 - num6 + 1)) + text.Substring(num7 + 1);
					i -= length - text.Length;
				}
			}
		}
		return text;
	}

	private static bool IsEntryOrRedirectToEntry(GlossaryEntry checking, GlossaryEntry ignore)
	{
		if (!(checking == ignore))
		{
			if (checking.IsRedirect && checking.LinkedEntries.Length != 0)
			{
				return checking.LinkedEntries[0] == ignore;
			}
			return false;
		}
		return true;
	}

	public static string StripGlossaryUrlTags(string text)
	{
		if (text != null)
		{
			string text2 = "url=glossary://";
			bool flag = false;
			int num = 0;
			int length = text.Length;
			while (num < length)
			{
				if (text[num] == '[')
				{
					int index = num;
					NGUITools.SymbolType symbolType = NGUITools.ParseSymbol(text, ref index, null, premultiply: false);
					bool flag2 = symbolType == NGUITools.SymbolType.URL_START && text.Length - (num + 1) >= text2.Length && text.Substring(num + 1, text2.Length) == text2;
					if (index > 0 && (flag2 || (symbolType == NGUITools.SymbolType.URL_END && flag)))
					{
						text = text.Remove(num, index);
						length = text.Length;
						continue;
					}
					if (flag2)
					{
						flag = true;
					}
					else if (symbolType == NGUITools.SymbolType.URL_END)
					{
						flag = false;
					}
				}
				num++;
			}
		}
		return text;
	}

	public string GetCategoryName(GlossaryCategory category)
	{
		string result = string.Empty;
		switch (category)
		{
		case GlossaryCategory.AfflictionsAndInjuries:
			result = GUIUtils.GetText(1719);
			break;
		case GlossaryCategory.CharacterStatistics:
			result = GUIUtils.GetText(1717);
			break;
		case GlossaryCategory.CombatMechanics:
			result = GUIUtils.GetText(1718);
			break;
		case GlossaryCategory.Equipment:
			result = GUIUtils.GetText(1720);
			break;
		case GlossaryCategory.Deities:
			result = GUIUtils.GetText(1936);
			break;
		case GlossaryCategory.Locations:
			result = GUIUtils.GetText(1937);
			break;
		case GlossaryCategory.Lore:
			result = GUIUtils.GetText(1938);
			break;
		case GlossaryCategory.Tips:
			result = GUIUtils.GetText(1962);
			break;
		}
		return result;
	}

	public int GetCategoryNameGuiId(GlossaryCategory category)
	{
		return category switch
		{
			GlossaryCategory.AfflictionsAndInjuries => 1719, 
			GlossaryCategory.CharacterStatistics => 1717, 
			GlossaryCategory.CombatMechanics => 1718, 
			GlossaryCategory.Equipment => 1720, 
			GlossaryCategory.Deities => 1936, 
			GlossaryCategory.Locations => 1937, 
			GlossaryCategory.Lore => 1938, 
			GlossaryCategory.Tips => 1962, 
			_ => -1, 
		};
	}

	private void BuildInvalidPrefixesAndSuffixes(string key)
	{
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = new StringBuilder();
		foreach (string key2 in entries.Keys)
		{
			int num = key2.IndexOf(key);
			if (num >= 0)
			{
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append("|");
				}
				stringBuilder.Append(Regex.Escape(key2.Substring(num + key.Length)));
				stringBuilder2.Append(Regex.Escape(key2.Substring(0, num)));
			}
		}
		string value = string.Empty;
		if (stringBuilder.Length > 0)
		{
			value = "(?!" + stringBuilder.ToString() + ")";
		}
		invalidSuffixes.Add(key, value);
		string value2 = string.Empty;
		if (stringBuilder2.Length > 0)
		{
			value2 = "(?<!" + stringBuilder2.ToString() + ")";
		}
		invalidPrefixes.Add(key, value2);
	}
}
