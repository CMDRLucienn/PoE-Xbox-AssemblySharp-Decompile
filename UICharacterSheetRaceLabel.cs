public class UICharacterSheetRaceLabel : UICharacterSheetContentLine
{
	private UILabel m_Label;

	private void Awake()
	{
		m_Label = GetComponent<UILabel>();
	}

	public override void Load(CharacterStats stats)
	{
		Gender gender = CharacterStats.GetGender(stats);
		if (stats.CharacterSubrace == CharacterStats.Subrace.Undefined)
		{
			m_Label.text = GUIUtils.GetRaceString(stats.CharacterRace, gender);
		}
		else
		{
			m_Label.text = GUIUtils.GetSubraceString(stats.CharacterSubrace, gender);
		}
		m_Label.text = UICharacterSheetContentLine.FormatPrefixed(GUIUtils.GetText(304, gender), m_Label.text);
	}
}
