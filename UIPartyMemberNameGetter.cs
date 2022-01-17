using UnityEngine;

[RequireComponent(typeof(UILabel))]
public class UIPartyMemberNameGetter : UIParentSelectorListener
{
	private UILabel m_Label;

	private void Awake()
	{
		m_Label = GetComponent<UILabel>();
	}

	public override void NotifySelectionChanged(CharacterStats stats)
	{
		m_Label.text = (stats ? CharacterStats.Name(stats) : "");
	}
}
