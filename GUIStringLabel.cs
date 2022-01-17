using System.Text.RegularExpressions;
using UnityEngine;

[ExecuteInEditMode]
public class GUIStringLabel : MonoBehaviour
{
	public GUIDatabaseString DatabaseString = new GUIDatabaseString();

	[Tooltip("If set, converst the string to all caps.")]
	public bool AllCaps;

	[Tooltip("If set, strings all format tokens like {0} out of the text.")]
	public bool RemoveFormatTokens;

	public string FormatString;

	public static GUIStringLabel Get(MonoBehaviour mb)
	{
		if ((bool)mb)
		{
			return Get(mb.gameObject);
		}
		return null;
	}

	public static GUIStringLabel Get(GameObject go)
	{
		if (!go)
		{
			return null;
		}
		GUIStringLabel gUIStringLabel = go.GetComponent<GUIStringLabel>();
		if (!gUIStringLabel)
		{
			gUIStringLabel = go.AddComponent<GUIStringLabel>();
		}
		return gUIStringLabel;
	}

	private void OnEnable()
	{
		RefreshText();
	}

	private void Start()
	{
		StringTableManager.OnLanguageChanged += OnLanguageChanged;
	}

	private void OnDestroy()
	{
		StringTableManager.OnLanguageChanged -= OnLanguageChanged;
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void OnLanguageChanged(Language lang)
	{
		RefreshText();
	}

	public void SetString(GUIDatabaseString str)
	{
		SetString(str.StringID);
	}

	public void SetString(int id)
	{
		DatabaseString.StringID = id;
		RefreshText();
	}

	public void RefreshText()
	{
		string text;
		if (DatabaseString == null)
		{
			text = "";
		}
		else
		{
			text = ((!AllCaps) ? DatabaseString.GetText() : DatabaseString.GetText().ToUpper());
			if (RemoveFormatTokens)
			{
				text = Regex.Replace(text, "{[0-9]+}", "").Trim();
			}
		}
		if (!string.IsNullOrEmpty(FormatString))
		{
			text = StringUtility.Format(FormatString, text);
		}
		UILabel component = GetComponent<UILabel>();
		if (component != null)
		{
			component.text = text;
		}
		UICapitularLabel component2 = GetComponent<UICapitularLabel>();
		if ((bool)component2)
		{
			component2.text = text;
		}
	}
}
