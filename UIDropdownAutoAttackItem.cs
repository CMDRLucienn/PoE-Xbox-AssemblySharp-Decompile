public class UIDropdownAutoAttackItem : UIDropdownItem
{
	private void OnHover(bool state)
	{
		if (state && m_Content is UICharacterAutoAttackDropdownData)
		{
			UICharacterAutoAttackDropdownData uICharacterAutoAttackDropdownData = (UICharacterAutoAttackDropdownData)m_Content;
			UIAiCustomizerManager.Instance.SetScriptTooltip(GUIUtils.GetAggressionTypeDesc(uICharacterAutoAttackDropdownData.Setting));
		}
	}
}
