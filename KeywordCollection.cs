using System;
using System.Collections.Generic;

public class KeywordCollection
{
	private List<string> m_keywords = new List<string>();

	private static char[] s_Comma = new char[1] { ',' };

	public bool Empty => m_keywords.Count == 0;

	public KeywordCollection(string keywordList)
	{
		string[] array = keywordList.Split(s_Comma, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = array[i].Trim().ToLowerInvariant();
		}
		m_keywords.AddRange(array);
	}

	public bool Contains(string keyword)
	{
		return m_keywords.Contains(keyword.ToLowerInvariant());
	}

	public string GetListString()
	{
		if (Empty)
		{
			return "";
		}
		string text = "";
		for (int i = 0; i < m_keywords.Count; i++)
		{
			DatabaseString adjective = KeywordData.GetAdjective(m_keywords[i]);
			if (adjective != null)
			{
				text = text + adjective.GetText() + GUIUtils.Comma();
			}
		}
		if (text.Length > 0)
		{
			text = text.Remove(text.Length - GUIUtils.Comma().Length);
		}
		return text;
	}
}
