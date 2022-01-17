using System;
using UnityEngine;

public class UIStashTab : MonoBehaviour
{
	public UIInventoryFilter.ItemFilterType FilterType;

	private UIMultiSpriteImageButton m_MultiButton;

	private UIInventoryStashGrid m_ForGrid;

	public UISprite SelectedSprite;

	public UISprite ButtonSprite;

	public bool Selected
	{
		get
		{
			return m_ForGrid.CurrentTab == FilterType;
		}
		set
		{
			if (value)
			{
				m_ForGrid.CurrentTab = FilterType;
			}
			else if (m_ForGrid.CurrentTab == FilterType)
			{
				m_ForGrid.CurrentTab = UIInventoryFilter.ItemFilterType.NONE;
			}
		}
	}

	private void Start()
	{
		m_ForGrid = GetComponentInParent<UIInventoryStashGrid>();
		m_MultiButton = GetComponent<UIMultiSpriteImageButton>();
		if (!m_MultiButton)
		{
			UIEventListener uIEventListener = UIEventListener.Get(ButtonSprite.gameObject);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnChildClicked));
			UIEventListener uIEventListener2 = UIEventListener.Get(ButtonSprite.gameObject);
			uIEventListener2.onTooltip = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener2.onTooltip, new UIEventListener.BoolDelegate(OnChildTooltip));
		}
		else
		{
			UIMultiSpriteImageButton multiButton = m_MultiButton;
			multiButton.onTooltip = (UIEventListener.BoolDelegate)Delegate.Combine(multiButton.onTooltip, new UIEventListener.BoolDelegate(OnChildTooltip));
		}
		UIInventoryStashGrid.OnTabChanged = (UIInventoryFilterManager.OnFiltersChanged)Delegate.Combine(UIInventoryStashGrid.OnTabChanged, new UIInventoryFilterManager.OnFiltersChanged(OnTabChanged));
		RefreshSelected();
	}

	private void OnDestroy()
	{
		if ((bool)m_ForGrid)
		{
			UIInventoryStashGrid.OnTabChanged = (UIInventoryFilterManager.OnFiltersChanged)Delegate.Remove(UIInventoryStashGrid.OnTabChanged, new UIInventoryFilterManager.OnFiltersChanged(OnTabChanged));
		}
	}

	private void OnClick()
	{
		Selected = !Selected;
	}

	private void OnChildClicked(GameObject sender)
	{
		OnClick();
	}

	private void OnTooltip(bool over)
	{
		if (over)
		{
			UIActionBarTooltip.GlobalShow(ButtonSprite, UIInventoryFilterManager.GetFilterTypeString(FilterType));
		}
		else
		{
			UIActionBarTooltip.GlobalHide();
		}
	}

	private void OnChildTooltip(GameObject sender, bool over)
	{
		OnTooltip(over);
	}

	private void OnTabChanged(UIInventoryFilter.ItemFilterType mask)
	{
		RefreshSelected();
	}

	public void RefreshSelected()
	{
		SelectedSprite.alpha = (Selected ? 1 : 0);
		if ((bool)m_MultiButton)
		{
			m_MultiButton.ForceDown(Selected);
		}
	}
}
