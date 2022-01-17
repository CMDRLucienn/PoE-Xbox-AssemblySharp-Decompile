using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class UIDynamicFontManager : MonoBehaviour
{
	[Serializable]
	public class FontPresetData
	{
		public UIDynamicFontSize.FontPreset Key;

		public FontClass Class;

		public FontStyle Style;

		public int Size;

		public bool AllowScaling;
	}

	public enum FontClass
	{
		ESP_REGULAR,
		[Obsolete("Use ESP_REGULAR with styles instead")]
		DEPRECATED_ESP_BOLD,
		[Obsolete("Use ESP_REGULAR with styles instead")]
		DEPRECATED_ESP_ITALIC,
		[Obsolete("Use ESP_REGULAR with styles instead")]
		DEPRECATED_ESP_BOLDITALIC,
		ESP_TITLING,
		ESP_CAPITULARIV,
		ESP_ORNAMENTS,
		CYRILLIC_REPLACEMENT,
		HANGUL_REPLACEMENT,
		ESP_BLACKSHAMROCK_BOLD
	}

	private struct FontIdentifier
	{
		public FontClass Class;

		public FontStyle Style;

		public int Size;

		public FontIdentifier(FontClass fclass, FontStyle fstyle, int size)
		{
			Class = fclass;
			Style = fstyle;
			Size = size;
		}
	}

	private class FontObject
	{
		public Font UnityFont;

		public UIFont NGUIFont;
	}

	public bool ClearCache;

	public UIFont EspRegular;

	public UIFont EspTitling;

	public UIFont EspCapitularIV;

	public UIFont EspOrnaments;

	public UIFont CyrillicReplacement;

	public UIFont HangulReplacement;

	public UIFont EspBlackShamrockBold;

	public float PreviewScale = 1f;

	public GameObject FontContainer;

	public FontPresetData[] Presets;

	[NonSerialized]
	private Dictionary<FontIdentifier, FontObject> m_InstantiatedFonts = new Dictionary<FontIdentifier, FontObject>();

	private static bool s_CurrentLanguageIsPolish;

	private static bool s_CurrentLanguageIsCyrillic;

	private static bool s_CurrentLanguageIsHangul;

	private static Language s_LastCheckedCurrentLanguage;

	private int m_OldScreenWidth;

	private int m_OldScreenHeight;

	public static UIDynamicFontManager Instance { get; private set; }

	public float NguiAdjustment { get; set; }

	public void CalculateNguiAdjustment()
	{
		if ((bool)InGameUILayout.Root)
		{
			NguiAdjustment = Mathf.Max(1f, (float)Screen.height / (float)InGameUILayout.Root.activeHeight);
		}
		else
		{
			NguiAdjustment = 1f;
		}
	}

	private void Start()
	{
		PreviewScale = 1f;
		Font.textureRebuilt += FontRebuiltCallback;
	}

	private void FontRebuiltCallback(Font font)
	{
		UILabel.MarkAllChanged(font);
	}

	public UIFont Get(UIDynamicFontSize.FontPreset preset)
	{
		FontPresetData[] presets = Presets;
		foreach (FontPresetData fontPresetData in presets)
		{
			if (fontPresetData.Key == preset)
			{
				return Get(fontPresetData.Class, fontPresetData.Style, fontPresetData.Size, fontPresetData.AllowScaling, allowFontOverride: true);
			}
		}
		Debug.LogError("DynamicFontManager: got request for preset '" + preset.ToString() + "' but no data mapped to that.");
		return null;
	}

	public UIFont Get(FontClass font, FontStyle style, int size, bool allowScaling, bool allowFontOverride)
	{
		if (FontContainer == null)
		{
			Debug.LogError("Font Manager has no FontContainer object. Creating one.");
			FontContainer = new GameObject();
			Transform obj = FontContainer.transform;
			obj.parent = base.transform.parent;
			obj.localScale = Vector3.one;
			FontContainer.name = "FontContainer";
		}
		if (size == 0)
		{
			size = 20;
		}
		if (s_LastCheckedCurrentLanguage != StringTableManager.CurrentLanguage)
		{
			s_LastCheckedCurrentLanguage = StringTableManager.CurrentLanguage;
			s_CurrentLanguageIsPolish = StringTableManager.CurrentLanguage.Name.Equals("polish");
			s_CurrentLanguageIsCyrillic = StringTableManager.CurrentLanguage.Charset == Language.CharacterSet.Cyrillic;
			s_CurrentLanguageIsHangul = StringTableManager.CurrentLanguage.Charset == Language.CharacterSet.Hangul;
		}
		switch (font)
		{
		case FontClass.DEPRECATED_ESP_BOLD:
			font = FontClass.ESP_BLACKSHAMROCK_BOLD;
			style = FontStyle.Normal;
			break;
		case FontClass.DEPRECATED_ESP_BOLDITALIC:
			font = FontClass.ESP_REGULAR;
			style = FontStyle.BoldAndItalic;
			break;
		case FontClass.DEPRECATED_ESP_ITALIC:
			font = FontClass.ESP_REGULAR;
			style = FontStyle.Italic;
			break;
		}
		if (allowFontOverride)
		{
			if (s_CurrentLanguageIsPolish)
			{
				if (font != FontClass.ESP_CAPITULARIV && font != FontClass.ESP_ORNAMENTS)
				{
					font = FontClass.ESP_REGULAR;
				}
				switch (style)
				{
				case FontStyle.BoldAndItalic:
					style = FontStyle.Italic;
					break;
				case FontStyle.Bold:
					style = FontStyle.Normal;
					break;
				}
				if (font == FontClass.ESP_BLACKSHAMROCK_BOLD)
				{
					font = FontClass.ESP_REGULAR;
				}
			}
			else if (s_CurrentLanguageIsCyrillic)
			{
				if (font != FontClass.ESP_ORNAMENTS)
				{
					font = FontClass.CYRILLIC_REPLACEMENT;
				}
			}
			else if (s_CurrentLanguageIsHangul && font != FontClass.ESP_ORNAMENTS)
			{
				font = FontClass.HANGUL_REPLACEMENT;
			}
		}
		if (Application.isPlaying)
		{
			if (allowScaling && GameState.Option != null)
			{
				size = Mathf.RoundToInt((float)size * GameState.Option.FontScale);
			}
		}
		else
		{
			size = Mathf.RoundToInt((float)size * PreviewScale);
		}
		FontObject value = null;
		FontIdentifier key = new FontIdentifier(font, style, size);
		if (m_InstantiatedFonts.TryGetValue(key, out value))
		{
			return value.NGUIFont;
		}
		UIFont baseFont = GetBaseFont(font);
		Font dynamicFont = baseFont.dynamicFont;
		FontObject fontObject = new FontObject();
		fontObject.UnityFont = dynamicFont;
		GameObject gameObject = new GameObject();
		gameObject.transform.parent = FontContainer.transform;
		gameObject.transform.localScale = Vector3.one;
		fontObject.NGUIFont = gameObject.AddComponent<UIFont>();
		fontObject.NGUIFont.dynamicFontSize = size;
		fontObject.NGUIFont.dynamicFontStyle = style;
		fontObject.NGUIFont.dynamicFont = fontObject.UnityFont;
		fontObject.NGUIFont.material = baseFont.material;
		fontObject.NGUIFont.gameObject.name = fontObject.UnityFont.name + "-sz" + size;
		fontObject.NGUIFont.horizontalSpacing = baseFont.horizontalSpacing;
		fontObject.NGUIFont.verticalSpacing = baseFont.verticalSpacing;
		MonoBehaviour[] components = baseFont.GetComponents<MonoBehaviour>();
		foreach (MonoBehaviour monoBehaviour in components)
		{
			if (!(monoBehaviour is UIFont))
			{
				ComponentUtils.CopyComponent(monoBehaviour, gameObject);
			}
		}
		m_InstantiatedFonts.Add(key, fontObject);
		return fontObject.NGUIFont;
	}

	public void Release(UIDynamicFontSize requester, UIDynamicFontSize.FontPreset preset)
	{
		FontPresetData[] presets = Presets;
		foreach (FontPresetData fontPresetData in presets)
		{
			if (fontPresetData.Key == preset)
			{
				Release(requester, fontPresetData.Class, fontPresetData.Style, fontPresetData.Size, fontPresetData.AllowScaling);
				return;
			}
		}
		Debug.LogError("DynamicFontManager: got release for preset '" + preset.ToString() + "' but no data mapped to that.");
	}

	public void Release(UIDynamicFontSize requester, FontClass font, FontStyle style, int size, bool allowScaling)
	{
	}

	private UIFont GetBaseFont(FontClass font)
	{
		return font switch
		{
			FontClass.ESP_REGULAR => EspRegular, 
			FontClass.ESP_TITLING => EspTitling, 
			FontClass.ESP_CAPITULARIV => EspCapitularIV, 
			FontClass.ESP_ORNAMENTS => EspOrnaments, 
			FontClass.CYRILLIC_REPLACEMENT => CyrillicReplacement, 
			FontClass.HANGUL_REPLACEMENT => HangulReplacement, 
			FontClass.ESP_BLACKSHAMROCK_BOLD => EspBlackShamrockBold, 
			_ => EspRegular, 
		};
	}

	private void Awake()
	{
		Instance = this;
		if (!Application.isPlaying)
		{
			ClearFontCache();
		}
		CalculateNguiAdjustment();
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		Font.textureRebuilt -= FontRebuiltCallback;
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Update()
	{
		if (ClearCache)
		{
			ClearFontCache();
		}
		if (m_OldScreenWidth == 0 && m_OldScreenHeight == 0)
		{
			m_OldScreenWidth = Screen.width;
			m_OldScreenHeight = Screen.height;
		}
		if (m_OldScreenWidth != Screen.width || m_OldScreenHeight != Screen.height)
		{
			m_OldScreenWidth = Screen.width;
			m_OldScreenHeight = Screen.height;
			UIDynamicFontSize.ReloadAllFonts();
		}
	}

	public void ClearFontCache()
	{
		if (!FontContainer)
		{
			return;
		}
		ClearCache = false;
		m_InstantiatedFonts.Clear();
		UIFont[] componentsInChildren = FontContainer.GetComponentsInChildren<UIFont>(includeInactive: true);
		foreach (UIFont uIFont in componentsInChildren)
		{
			if (uIFont.gameObject != null)
			{
				if (Application.isPlaying)
				{
					GameUtilities.Destroy(uIFont.gameObject);
				}
				else
				{
					GameUtilities.DestroyImmediate(uIFont.gameObject);
				}
			}
		}
	}
}
