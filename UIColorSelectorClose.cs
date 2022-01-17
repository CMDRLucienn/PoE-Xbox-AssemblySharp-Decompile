using System;
using UnityEngine;

public class UIColorSelectorClose : MonoBehaviour
{
	private UIMultiSpriteImageButton m_Button;

	public UIColorSelectorLine SelectorLine;

	private void Start()
	{
		m_Button = GetComponent<UIMultiSpriteImageButton>();
		UIMultiSpriteImageButton button = m_Button;
		button.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(button.onClick, new UIEventListener.VoidDelegate(OnButtonClick));
	}

	private void OnButtonClick(GameObject sender)
	{
		SelectorLine.Hide();
	}
}
