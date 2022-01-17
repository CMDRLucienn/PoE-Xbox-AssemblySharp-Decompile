using System;
using UnityEngine;

public class UIRadioButtonGroup : MonoBehaviour
{
	public delegate void RadioSelectionChanged(UIMultiSpriteImageButton selected);

	private UIMultiSpriteImageButton[] m_Buttons;

	public event RadioSelectionChanged OnRadioSelectionChanged;

	public void Reinitialize()
	{
		if (m_Buttons != null)
		{
			UIMultiSpriteImageButton[] buttons = m_Buttons;
			foreach (UIMultiSpriteImageButton obj in buttons)
			{
				obj.onClick = (UIEventListener.VoidDelegate)Delegate.Remove(obj.onClick, new UIEventListener.VoidDelegate(OnRadioClicked));
			}
			m_Buttons = null;
		}
		Init();
	}

	private void Start()
	{
		Init();
	}

	private void Init()
	{
		if (m_Buttons == null)
		{
			m_Buttons = GetComponentsInChildren<UIMultiSpriteImageButton>(includeInactive: true);
			UIMultiSpriteImageButton[] buttons = m_Buttons;
			foreach (UIMultiSpriteImageButton obj in buttons)
			{
				obj.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(obj.onClick, new UIEventListener.VoidDelegate(OnRadioClicked));
			}
			if (m_Buttons.Length != 0)
			{
				OnRadioClicked(m_Buttons[0].gameObject);
			}
		}
	}

	public void DoSelect(GameObject button)
	{
		OnRadioClicked(button);
	}

	private void OnRadioClicked(GameObject sender)
	{
		Init();
		UIMultiSpriteImageButton[] buttons = m_Buttons;
		foreach (UIMultiSpriteImageButton uIMultiSpriteImageButton in buttons)
		{
			if ((bool)uIMultiSpriteImageButton.Label && uIMultiSpriteImageButton.gameObject == sender)
			{
				uIMultiSpriteImageButton.Label.color = UIGlobalColor.Instance.TabSelected;
			}
			else
			{
				uIMultiSpriteImageButton.Label.color = UIGlobalColor.Instance.TabUnselected;
			}
		}
		if (this.OnRadioSelectionChanged != null)
		{
			this.OnRadioSelectionChanged(sender.GetComponent<UIMultiSpriteImageButton>());
		}
	}
}
