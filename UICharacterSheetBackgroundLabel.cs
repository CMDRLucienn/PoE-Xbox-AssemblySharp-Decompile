public class UICharacterSheetBackgroundLabel : UICharacterSheetContentLine
{
	private UILabel m_Label;

	private void Awake()
	{
		m_Label = GetComponent<UILabel>();
	}

	public override void Load(CharacterStats stats)
	{
		Gender gender = CharacterStats.GetGender(stats);
		if (stats.CharacterCulture != 0 && stats.CharacterBackground != 0)
		{
			m_Label.text = GUIUtils.GetCultureString(stats.CharacterCulture, gender) + " - " + GUIUtils.GetBackgroundString(stats.CharacterBackground, gender);
		}
		else if (stats.CharacterCulture != 0)
		{
			m_Label.text = GUIUtils.GetCultureString(stats.CharacterCulture, gender);
		}
		else if (stats.CharacterBackground != 0)
		{
			m_Label.text = GUIUtils.GetBackgroundString(stats.CharacterBackground, gender);
		}
		else
		{
			m_Label.text = "";
		}
		if (!string.IsNullOrEmpty(m_Label.text))
		{
			m_Label.text = UICharacterSheetContentLine.FormatPrefixed(GUIUtils.GetText(307, gender), m_Label.text);
		}
	}
}
