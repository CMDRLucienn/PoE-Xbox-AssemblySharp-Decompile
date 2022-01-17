using UnityEngine;

public class UIDropdownLanguageItem : UIDropdownItem
{
	private UIDynamicFontSize m_FontSize;

	public override void NotifyContentChanged()
	{
		if (!m_FontSize)
		{
			m_FontSize = GetComponent<UIDynamicFontSize>();
		}
		if (m_Content is Language language)
		{
			switch (language.Charset)
			{
			case Language.CharacterSet.Latin:
				m_FontSize.Class = UIDynamicFontManager.FontClass.ESP_REGULAR;
				m_FontSize.DoNotOverrideFont = true;
				base.transform.localPosition = new Vector3(base.transform.localPosition.x, 0f, base.transform.localPosition.z);
				UIDynamicFontSize.Guarantee(m_FontSize);
				break;
			case Language.CharacterSet.Cyrillic:
				m_FontSize.Class = UIDynamicFontManager.FontClass.CYRILLIC_REPLACEMENT;
				m_FontSize.DoNotOverrideFont = true;
				base.transform.localPosition = new Vector3(base.transform.localPosition.x, 0f, base.transform.localPosition.z);
				UIDynamicFontSize.Guarantee(m_FontSize);
				break;
			case Language.CharacterSet.Hangul:
				m_FontSize.Class = UIDynamicFontManager.FontClass.HANGUL_REPLACEMENT;
				m_FontSize.DoNotOverrideFont = true;
				base.transform.localPosition = new Vector3(base.transform.localPosition.x, -5f, base.transform.localPosition.z);
				UIDynamicFontSize.Guarantee(m_FontSize);
				break;
			}
		}
	}
}
