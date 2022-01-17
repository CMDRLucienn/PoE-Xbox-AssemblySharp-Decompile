using System;
using System.Linq;
using UnityEngine;

public class KeywordData : MonoBehaviour
{
	[Serializable]
	public class Keyword
	{
		public string KeywordTag;

		public GUIDatabaseString DatabaseString;

		public GUIDatabaseString OverridePluralEffect;

		public Texture2D EffectIcon;
	}

	public Keyword[] Data;

	public static KeywordData Instance { get; private set; }

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Debug.LogError("Singleton component 'KeywordData' awoke multiple times! Remove this component from all objects except global prefabs. (Second object: '" + base.name + "')");
		}
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	public static Keyword GetData(string keyword)
	{
		Keyword keyword2 = Instance.Data.FirstOrDefault((Keyword kd) => kd.KeywordTag.Equals(keyword, StringComparison.OrdinalIgnoreCase));
		if (keyword2 == null)
		{
			Debug.LogError("Tried to look up keyword '" + keyword + "' but that keyword doesn't exist.");
		}
		return keyword2;
	}

	public static Texture2D GetIcon(string keyword)
	{
		return GetData(keyword)?.EffectIcon;
	}

	public static GUIDatabaseString GetAdjective(string keyword)
	{
		return GetData(keyword)?.DatabaseString;
	}

	public static string GetNoun(string keyword)
	{
		Keyword data = GetData(keyword);
		if (data == null)
		{
			return "*KeywordError*";
		}
		if (data.DatabaseString.IsValidString)
		{
			return data.DatabaseString.GetText();
		}
		return GUIUtils.Format(1504, data.DatabaseString.GetText());
	}

	public static string GetNounPlural(string keyword)
	{
		Keyword data = GetData(keyword);
		if (data == null)
		{
			return "*KeywordError*";
		}
		if (data.OverridePluralEffect.IsValidString)
		{
			return data.OverridePluralEffect.GetText();
		}
		return GUIUtils.Format(1505, data.DatabaseString.GetText());
	}
}
