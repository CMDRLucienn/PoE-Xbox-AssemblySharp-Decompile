using System;
using UnityEngine;

public class UIInventoryFilter : MonoBehaviour
{
	[Flags]
	public enum ItemFilterType
	{
		NONE = 0,
		WEAPONS = 1,
		ARMOR = 2,
		DEPRECATED_AMMO = 4,
		CLOTHING = 8,
		CONSUMABLES = 0x10,
		INGREDIENTS = 0x20,
		QUEST = 0x40,
		MISC = 0x80
	}

	public const int COUNT = 8;

	public const ItemFilterType ALL = ItemFilterType.WEAPONS | ItemFilterType.ARMOR | ItemFilterType.DEPRECATED_AMMO | ItemFilterType.CLOTHING | ItemFilterType.CONSUMABLES | ItemFilterType.INGREDIENTS | ItemFilterType.QUEST | ItemFilterType.MISC;

	public ItemFilterType FilterType;

	private UIMultiSpriteImageButton m_MultiButton;

	public UISprite SelectedSprite;

	public UISprite ButtonSprite;

	public bool Selected
	{
		get
		{
			return UIInventoryFilterManager.GetFilter(FilterType);
		}
		set
		{
			UIInventoryFilterManager.SetFilter(FilterType, value);
		}
	}

	private void Start()
	{
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
		UIInventoryFilterManager.FilterChanged = (UIInventoryFilterManager.OnFiltersChanged)Delegate.Combine(UIInventoryFilterManager.FilterChanged, new UIInventoryFilterManager.OnFiltersChanged(OnFilterChanged));
		RefreshSelected();
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

	private void OnFilterChanged(ItemFilterType mask)
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
