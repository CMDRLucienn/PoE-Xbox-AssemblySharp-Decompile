using UnityEngine;

public class UICharacterCreationColorSelectorCategory : MonoBehaviour
{
	public UICharacterCreationColorSelector m_leftColorSelector;

	public UICharacterCreationColorSelector m_rightColorSelector;

	private bool m_lastActiveStatus = true;

	private void Update()
	{
		bool flag = m_leftColorSelector.IsSelectorEnabled() || m_rightColorSelector.IsSelectorEnabled();
		if (flag != m_lastActiveStatus)
		{
			m_lastActiveStatus = flag;
			GetComponent<UISprite>().alpha = (m_lastActiveStatus ? 1f : 0f);
		}
	}
}
