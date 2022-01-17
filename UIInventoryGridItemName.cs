using System;

public class UIInventoryGridItemName : UIParentDependentBehaviour
{
	public UILabel Label;

	private UIInventoryGridItem m_Owner;

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
			Label.text = item.InvItem.baseItem.Name;
		}
		else
		{
			Label.text = "";
		}
	}
}
