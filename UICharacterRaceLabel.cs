public class UICharacterRaceLabel : UIParentSelectorListener
{
	private UILabel m_Label;

	public bool SubstituteSubrace;

	public bool IgnoreKith;

	private void Awake()
	{
		m_Label = GetComponent<UILabel>();
	}

	public override void NotifySelectionChanged(CharacterStats stats)
	{
		if ((bool)stats)
		{
			if (IgnoreKith && CharacterStats.IsKithRace(stats.CharacterRace))
			{
				m_Label.text = "";
			}
			else if (SubstituteSubrace && stats.CharacterSubrace != 0)
			{
				m_Label.text = GUIUtils.GetSubraceString(stats.CharacterSubrace, CharacterStats.GetGender(stats));
			}
			else
			{
				m_Label.text = GUIUtils.GetRaceString(stats.CharacterRace, CharacterStats.GetGender(stats));
			}
		}
		else
		{
			m_Label.text = "";
		}
	}
}
