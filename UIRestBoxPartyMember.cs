using UnityEngine;

public class UIRestBoxPartyMember : MonoBehaviour, ISelectACharacter
{
	public UIDropdownMenu BonusDropdown;

	public UIIsButton DropdownTriggerButton;

	public CharacterStats SelectedCharacter { get; private set; }

	public event SelectedCharacterChanged OnSelectedCharacterChanged;

	public void LoadPartyMember(PartyMemberAI partyMember)
	{
		if ((bool)partyMember)
		{
			CharacterStats selection = (SelectedCharacter = partyMember.GetComponent<CharacterStats>());
			if (this.OnSelectedCharacterChanged != null)
			{
				this.OnSelectedCharacterChanged(selection);
			}
			DropdownTriggerButton.enabled = SelectedCharacter.CalculateSkill(CharacterStats.SkillType.Survival) > 0;
			if (!DropdownTriggerButton.enabled)
			{
				BonusDropdown.ForceShowObject(GUIUtils.GetText(343));
			}
			base.gameObject.SetActive(value: true);
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
	}
}
