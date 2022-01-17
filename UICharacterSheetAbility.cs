using UnityEngine;

public class UICharacterSheetAbility : MonoBehaviour
{
	public UILabel Label;

	private GenericAbility m_Ability;

	private void OnClick()
	{
		UIItemInspectManager.Examine(m_Ability);
	}

	public void SetAbility(GenericAbility abil)
	{
		m_Ability = abil;
		Label.text = GenericAbility.Name(m_Ability);
	}

	public void SetNone()
	{
		m_Ability = null;
		Label.text = GUIUtils.GetText(343);
	}
}
