public class UICharacterSheetNameLabel : UICharacterSheetContentLine
{
	private UILabel m_Label;

	private void Awake()
	{
		m_Label = GetComponent<UILabel>();
	}

	public override void Load(CharacterStats stats)
	{
		m_Label.text = UICharacterSheetContentLine.FormatPrefixed(GUIUtils.GetText(311, CharacterStats.GetGender(stats)), CharacterStats.Name(stats));
	}
}
