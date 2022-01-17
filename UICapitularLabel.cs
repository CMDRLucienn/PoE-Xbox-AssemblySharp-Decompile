using System;
using UnityEngine;

public class UICapitularLabel : MonoBehaviour
{
	public delegate void TextChanged(string newtext);

	public UILabel CapitalLabel;

	public UILabel LowerLabel;

	public UILabel LowerLabel2;

	public UILabel TextLabel;

	public UICapitularLabel StealTextFrom;

	private UIStretch m_textLabelUIStretch;

	private bool m_IlluminationEnabled = true;

	public bool IlluminationEnabled
	{
		get
		{
			return m_IlluminationEnabled;
		}
		set
		{
			m_IlluminationEnabled = value;
		}
	}

	public string text
	{
		get
		{
			string text = "";
			if ((bool)CapitalLabel)
			{
				text += CapitalLabel.text;
			}
			else if ((bool)LowerLabel)
			{
				text += LowerLabel.text;
			}
			if ((bool)TextLabel)
			{
				text += TextLabel.text;
			}
			return text;
		}
		set
		{
			string text = value;
			if (value == null)
			{
				text = "";
			}
			if (this.OnTextChanged != null)
			{
				this.OnTextChanged(text);
			}
			text = Prepare(text);
			bool flag = text.Length > 0 && (text[0] < 'a' || text[0] > 'z') && (text[0] < 'A' || text[0] > 'Z');
			if ((text.Length > 0 && text[0] >= '0' && text[0] <= '9') || !IlluminationEnabled)
			{
				if ((bool)CapitalLabel)
				{
					CapitalLabel.text = '\u0002'.ToString();
				}
				if ((bool)LowerLabel)
				{
					LowerLabel.text = "";
				}
				if ((bool)LowerLabel2)
				{
					LowerLabel2.text = "";
				}
				if ((bool)TextLabel)
				{
					TextLabel.text = value;
				}
			}
			else if (text.Length > 0)
			{
				if ((bool)CapitalLabel && text.Length > 0)
				{
					CapitalLabel.text = text[0].ToString().ToUpper();
				}
				if ((bool)LowerLabel && text.Length > 0)
				{
					LowerLabel.text = (flag ? "" : text[0].ToString().ToLower());
				}
				if ((bool)LowerLabel2 && text.Length > 0)
				{
					LowerLabel2.text = LowerLabel.text;
				}
				if ((bool)TextLabel && text.Length > 0)
				{
					TextLabel.text = text.Substring(1);
				}
			}
			else
			{
				if ((bool)CapitalLabel)
				{
					CapitalLabel.text = "";
				}
				if ((bool)LowerLabel)
				{
					LowerLabel.text = "";
				}
				if ((bool)LowerLabel2)
				{
					LowerLabel2.text = "";
				}
				if ((bool)TextLabel)
				{
					TextLabel.text = "";
				}
			}
			RefreshLabels();
		}
	}

	public event TextChanged OnTextChanged;

	public string Prepare(string str)
	{
		char[] trimChars = new char[26]
		{
			'"', '\'', '[', ' ', '\t', '\n', '\r', '*', '.', ',',
			'¡', '¿', '“', '”', '„', '«', '»', '「', '」', '『',
			'』', '‹', '›', '‘', '’', '‚'
		};
		return str.TrimStart(trimChars);
	}

	private void Awake()
	{
		if ((bool)StealTextFrom)
		{
			StealTextFrom.OnTextChanged += OnStealText;
		}
		GameMode option = GameState.Option;
		option.OnFontScaleChanged = (GameMode.FontScaleChangedDelegate)Delegate.Combine(option.OnFontScaleChanged, new GameMode.FontScaleChangedDelegate(OnFontScaleChangedAll));
		StringTableManager.OnLanguageChanged += OnLanguageChangedAll;
		if ((bool)TextLabel)
		{
			m_textLabelUIStretch = TextLabel.GetComponent<UIStretch>();
		}
		RefreshLabels();
	}

	private void OnDestroy()
	{
		if ((bool)StealTextFrom)
		{
			StealTextFrom.OnTextChanged -= OnStealText;
		}
		GameMode option = GameState.Option;
		option.OnFontScaleChanged = (GameMode.FontScaleChangedDelegate)Delegate.Remove(option.OnFontScaleChanged, new GameMode.FontScaleChangedDelegate(OnFontScaleChangedAll));
		StringTableManager.OnLanguageChanged -= OnLanguageChangedAll;
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Update()
	{
		if (!TextLabel || (!CapitalLabel && !LowerLabel))
		{
			return;
		}
		float y = TextLabel.transform.localPosition.y;
		float num = 0f;
		if ((bool)CapitalLabel)
		{
			num = CapitalLabel.transform.localScale.y;
		}
		else if ((bool)LowerLabel)
		{
			num = LowerLabel.transform.localScale.y;
		}
		y = ((StringTableManager.CurrentLanguage == null || StringTableManager.CurrentLanguage.Charset == Language.CharacterSet.Latin) ? (y - num * 0.55f) : (y - num * 1f));
		y = Mathf.Floor(y);
		if ((bool)TextLabel)
		{
			TextLabel.indentFirst = (int)Mathf.Abs(y);
			if (StringTableManager.CurrentLanguage != null && StringTableManager.CurrentLanguage.Charset != 0)
			{
				if ((CapitalLabel.text.Length > 0 && CapitalLabel.text[0] == 'Д') || StringTableManager.CurrentLanguage.Charset == Language.CharacterSet.Hangul)
				{
					TextLabel.indentFirst = (int)((float)TextLabel.indentFirst * 0.9f);
				}
				else
				{
					TextLabel.indentFirst = (int)((float)TextLabel.indentFirst * 0.75f);
				}
			}
		}
		if ((bool)CapitalLabel)
		{
			CapitalLabel.transform.localPosition = new Vector3(CapitalLabel.transform.localPosition.x, y, CapitalLabel.transform.localPosition.z);
		}
		if ((bool)LowerLabel)
		{
			LowerLabel.transform.localPosition = new Vector3(LowerLabel.transform.localPosition.x, y, LowerLabel.transform.localPosition.z);
		}
		if ((bool)LowerLabel2)
		{
			LowerLabel2.transform.localPosition = new Vector3(LowerLabel2.transform.localPosition.x, y, LowerLabel2.transform.localPosition.z);
		}
	}

	private void OnStealText(string newtext)
	{
		text = newtext;
	}

	private void OnFontScaleChangedAll(float scale)
	{
		RefreshLabels();
	}

	private void OnLanguageChangedAll(Language lang)
	{
		RefreshLabels();
	}

	public void RefreshLabels()
	{
		if ((bool)TextLabel)
		{
			UIDynamicFontSize.Guarantee(TextLabel.GetComponent<UIDynamicFontSize>());
		}
		if ((bool)CapitalLabel)
		{
			UIDynamicFontSize.Guarantee(CapitalLabel.GetComponent<UIDynamicFontSize>());
		}
		if ((bool)LowerLabel)
		{
			UIDynamicFontSize.Guarantee(LowerLabel.GetComponent<UIDynamicFontSize>());
		}
		if ((bool)LowerLabel2)
		{
			UIDynamicFontSize.Guarantee(LowerLabel2.GetComponent<UIDynamicFontSize>());
		}
		Update();
		if ((bool)m_textLabelUIStretch)
		{
			m_textLabelUIStretch.Update();
		}
	}
}
