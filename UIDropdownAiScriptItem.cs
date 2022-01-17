public class UIDropdownAiScriptItem : UIDropdownItem
{
	private void OnHover(bool state)
	{
		if (state && m_Content is UICharacterAiScriptDropdown.AiScriptDropdownItem)
		{
			UICharacterAiScriptDropdown.AiScriptDropdownItem aiScriptDropdownItem = (UICharacterAiScriptDropdown.AiScriptDropdownItem)m_Content;
			if ((bool)aiScriptDropdownItem.Script)
			{
				UIAiCustomizerManager.Instance.SetScriptTooltip(aiScriptDropdownItem.Script.Description.GetText());
			}
			else
			{
				UIAiCustomizerManager.Instance.SetScriptTooltip(GUIUtils.GetText(2267));
			}
		}
	}
}
