public class UICharacterSheetClassLabel : UIParentSelectorListener
{
	private UILabel m_Label;

	private void Awake()
	{
		m_Label = GetComponent<UILabel>();
	}

	public override void NotifySelectionChanged(CharacterStats stats)
	{
		if ((bool)stats)
		{
			string text = GUIUtils.Format(374, Ordinal.Get(stats.ScaledLevel), GUIUtils.GetClassString(stats.CharacterClass, stats.Gender));
			if (stats.CharacterClass == CharacterStats.Class.Paladin)
			{
				text += GUIUtils.Format(1731, GUIUtils.GetPaladinOrderString(stats.PaladinOrder, stats.Gender));
			}
			else if (stats.CharacterClass == CharacterStats.Class.Priest)
			{
				text += GUIUtils.Format(1731, GUIUtils.GetDeityString(stats.Deity));
			}
			m_Label.text = UICharacterSheetContentManager.FormatPrefixed(GUIUtils.GetText(305, stats.Gender), text);
		}
		else
		{
			m_Label.text = "";
		}
	}
}
