using UnityEngine;

public class UIPartyMemberSecondaryWeaponWidget : UIParentSelectorListener
{
	private CharacterStats m_NeedsReload;

	public GameObject HideShow;

	private void LateUpdate()
	{
		if ((bool)m_NeedsReload)
		{
			ReloadCharacter(m_NeedsReload);
			m_NeedsReload = null;
		}
	}

	public override void NotifySelectionChanged(CharacterStats character)
	{
		m_NeedsReload = character;
	}

	private void ReloadCharacter(CharacterStats stats)
	{
		Equipment component = stats.GetComponent<Equipment>();
		HideShow.SetActive(component.SecondaryAttack);
	}
}
