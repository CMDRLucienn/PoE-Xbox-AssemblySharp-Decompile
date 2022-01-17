using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class UIDynamicFontSize : MonoBehaviour
{
	public enum FontPreset
	{
		MANUAL,
		BLOCKTEXT,
		LINETEXT,
		BUTTON,
		TITLEBAR,
		MAPLABEL,
		MAPLABEL_LARGE,
		CAPITULAR,
		BUTTON_SMALL,
		OVERLAY_SCALES
	}

	public FontPreset Preset;

	public UIDynamicFontManager.FontClass Class;

	public FontStyle Style;

	public int Size = 20;

	public bool AllowScaling = true;

	private UILabel m_Label;

	private static Dictionary<UIDynamicFontSize, UIDynamicFontSize> s_AllUIDynamicFontSizes;

	public bool DoNotOverrideFont { get; set; }

	static UIDynamicFontSize()
	{
		s_AllUIDynamicFontSizes = new Dictionary<UIDynamicFontSize, UIDynamicFontSize>(2000);
		GameMode option = GameState.Option;
		option.OnFontScaleChanged = (GameMode.FontScaleChangedDelegate)Delegate.Combine(option.OnFontScaleChanged, new GameMode.FontScaleChangedDelegate(OnFontScaleChangedAll));
		StringTableManager.OnLanguageChanged += OnLanguageChangedAll;
	}

	public static void Cleanup()
	{
		if (GameState.Option != null)
		{
			GameMode option = GameState.Option;
			option.OnFontScaleChanged = (GameMode.FontScaleChangedDelegate)Delegate.Remove(option.OnFontScaleChanged, new GameMode.FontScaleChangedDelegate(OnFontScaleChangedAll));
		}
		StringTableManager.OnLanguageChanged -= OnLanguageChangedAll;
	}

	private void Start()
	{
		if (Application.isPlaying)
		{
			s_AllUIDynamicFontSizes.Add(this, this);
		}
		OnEnable();
	}

	private void OnEnable()
	{
		if (UIDynamicFontManager.Instance != null)
		{
			SetFont();
		}
	}

	private void OnDestroy()
	{
		if (Application.isPlaying)
		{
			s_AllUIDynamicFontSizes.Remove(this);
		}
	}

	private static void OnLanguageChangedAll(Language lang)
	{
		ReloadAllFonts();
	}

	private static void OnFontScaleChangedAll(float scale)
	{
		ReloadAllFonts();
	}

	public static void ReloadAllFonts()
	{
		if (!UIDynamicFontManager.Instance)
		{
			return;
		}
		UIDynamicFontManager.Instance.CalculateNguiAdjustment();
		UIDynamicFontManager.Instance.ClearFontCache();
		foreach (KeyValuePair<UIDynamicFontSize, UIDynamicFontSize> s_AllUIDynamicFontSize in s_AllUIDynamicFontSizes)
		{
			if (s_AllUIDynamicFontSize.Value != null)
			{
				s_AllUIDynamicFontSize.Value.SetFont();
			}
		}
	}

	public static void Guarantee(GameObject go)
	{
		go.GetComponent<UIDynamicFontSize>().SetFont();
	}

	public static void Guarantee(UIDynamicFontSize fs)
	{
		if (fs != null)
		{
			fs.SetFont();
		}
	}

	private void SetFont()
	{
		if (m_Label == null)
		{
			m_Label = GetComponent<UILabel>();
		}
		if (!(m_Label == null))
		{
			Release();
			UIFont uIFont = Get();
			if (uIFont != null && m_Label != null)
			{
				m_Label.font = uIFont;
				m_Label.transform.localScale = new Vector3(uIFont.size, uIFont.size, 1f);
				m_Label.MarkAsChanged();
			}
		}
	}

	private UIFont Get()
	{
		if ((bool)UIDynamicFontManager.Instance)
		{
			if (Preset == FontPreset.MANUAL)
			{
				if (Style == FontStyle.Bold)
				{
					Style = FontStyle.Normal;
					Class = UIDynamicFontManager.FontClass.ESP_BLACKSHAMROCK_BOLD;
				}
				return UIDynamicFontManager.Instance.Get(Class, Style, Size, AllowScaling, !DoNotOverrideFont);
			}
			return UIDynamicFontManager.Instance.Get(Preset);
		}
		return null;
	}

	private void Release()
	{
		if ((bool)UIDynamicFontManager.Instance)
		{
			if (Preset == FontPreset.MANUAL)
			{
				UIDynamicFontManager.Instance.Release(this, Class, Style, Size, AllowScaling);
			}
			else
			{
				UIDynamicFontManager.Instance.Release(this, Preset);
			}
		}
	}
}
