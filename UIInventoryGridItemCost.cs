using System;

public class UIInventoryGridItemCost : UIParentDependentBehaviour
{
	public UILabel Label;

	private UIInventoryGridItem m_Owner;

	public int Cost
	{
		get
		{
			if (m_Owner.InvItem == null || m_Owner.InvItem.baseItem == null)
			{
				return 0;
			}
			return (int)m_Owner.InvItem.baseItem.GetBuyValue(UIStoreManager.Instance.Store);
		}
	}

	private void OnEnable()
	{
		FindParent();
	}

	private void Awake()
	{
		if (Label == null)
		{
			Label = GetComponent<UILabel>();
		}
	}

	private void OnDestroy()
	{
		if ((bool)m_Owner)
		{
			UIInventoryGridItem owner = m_Owner;
			owner.OnItemReload = (UIInventoryItemZone.GridItemDelegate)Delegate.Remove(owner.OnItemReload, new UIInventoryItemZone.GridItemDelegate(Reload));
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	public override void FindParent()
	{
		if ((bool)m_Owner)
		{
			UIInventoryGridItem owner = m_Owner;
			owner.OnItemReload = (UIInventoryItemZone.GridItemDelegate)Delegate.Remove(owner.OnItemReload, new UIInventoryItemZone.GridItemDelegate(Reload));
		}
		m_Owner = GetComponentInParent<UIInventoryGridItem>();
		if ((bool)m_Owner)
		{
			UIInventoryGridItem owner2 = m_Owner;
			owner2.OnItemReload = (UIInventoryItemZone.GridItemDelegate)Delegate.Combine(owner2.OnItemReload, new UIInventoryItemZone.GridItemDelegate(Reload));
		}
		Reload(m_Owner);
	}

	private void Reload(UIInventoryGridItem item)
	{
		if (item != null && item.InvItem != null && (bool)item.InvItem.baseItem)
		{
			Label.text = GUIUtils.Format(466, Cost);
		}
		else
		{
			Label.text = "";
		}
	}
}
