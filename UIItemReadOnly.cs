using System;
using UnityEngine;

public class UIItemReadOnly : MonoBehaviour
{
	public UITexture IconTexture;

	public GameObject Collider;

	public UILabel QuantityLabel;

	private Item m_Item;

	private int m_Quantity;

	private void Start()
	{
		UIEventListener uIEventListener = UIEventListener.Get(Collider);
		uIEventListener.onTooltip = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onTooltip, new UIEventListener.BoolDelegate(OnChildTooltip));
		UIEventListener uIEventListener2 = UIEventListener.Get(Collider);
		uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, new UIEventListener.VoidDelegate(OnChildClick));
		UIEventListener uIEventListener3 = UIEventListener.Get(Collider);
		uIEventListener3.onRightClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener3.onRightClick, new UIEventListener.VoidDelegate(OnChildClick));
	}

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void OnChildTooltip(GameObject sender, bool over)
	{
		if (over && (bool)m_Item)
		{
			UIAbilityTooltip.GlobalShow(IconTexture, m_Item);
		}
		else
		{
			UIAbilityTooltip.GlobalHide();
		}
	}

	private void OnChildClick(GameObject sender)
	{
		if ((bool)m_Item)
		{
			UIItemInspectManager.ExamineStore(m_Item, null);
		}
	}

	public void LoadItem(Item item)
	{
		LoadItem(item, 1);
	}

	public void LoadItem(Item item, int quantity)
	{
		m_Item = item;
		m_Quantity = quantity;
		if ((bool)IconTexture)
		{
			IconTexture.mainTexture = (m_Item ? m_Item.GetIconTexture() : null);
		}
		if ((bool)QuantityLabel)
		{
			if (m_Quantity != 1)
			{
				QuantityLabel.text = m_Quantity.ToString();
			}
			else
			{
				QuantityLabel.text = "";
			}
		}
	}

	public void LoadItem(InventoryItem item)
	{
		LoadItem(item.baseItem, item.stackSize);
	}
}
