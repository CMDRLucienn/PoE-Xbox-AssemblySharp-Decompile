using System;
using UnityEngine;

public class UIInventoryStashGrid : MonoBehaviour
{
	public UIInventoryItemGrid ItemGrid;

	private UIInventoryFilter.ItemFilterType m_CurrentTab;

	private UIDropdownInventorySort.InventorySortChoice m_currentSort;

	public static UIInventoryFilterManager.OnFiltersChanged OnTabChanged;

	public UIInventoryFilter.ItemFilterType CurrentTab
	{
		get
		{
			return m_CurrentTab;
		}
		set
		{
			if (value != 0 && value != UIInventoryFilter.ItemFilterType.DEPRECATED_AMMO)
			{
				m_CurrentTab = value;
				if (OnTabChanged != null)
				{
					OnTabChanged(CurrentTab);
				}
				Reload();
			}
		}
	}

	public UIDropdownInventorySort.InventorySortChoice CurrentSorting
	{
		get
		{
			return m_currentSort;
		}
		set
		{
			m_currentSort = value;
			ItemGrid.SortFunc = m_currentSort.SortFunc;
			Reload();
		}
	}

	private void Awake()
	{
		UIInventoryItemGrid itemGrid = ItemGrid;
		itemGrid.OnItemPut = (UIInventoryItemZone.ItemSwapDelegate)Delegate.Combine(itemGrid.OnItemPut, new UIInventoryItemZone.ItemSwapDelegate(OnItemPut));
	}

	private void OnItemPut(Item baseItem, int quantity)
	{
		CurrentTab = baseItem.FilterType;
	}

	private void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
		if ((bool)ItemGrid)
		{
			UIInventoryItemGrid itemGrid = ItemGrid;
			itemGrid.OnItemPut = (UIInventoryItemZone.ItemSwapDelegate)Delegate.Remove(itemGrid.OnItemPut, new UIInventoryItemZone.ItemSwapDelegate(OnItemPut));
		}
	}

	private void Start()
	{
		CurrentTab = UIInventoryFilter.ItemFilterType.WEAPONS;
	}

	private void Reload()
	{
		if (CurrentTab == UIInventoryFilter.ItemFilterType.INGREDIENTS)
		{
			ItemGrid.Locked = !UIStoreManager.Instance.WindowActive();
			ItemGrid.InclusionFilter = null;
			ItemGrid.LoadInventory(GameState.s_playerCharacter.GetComponent<CraftingInventory>());
			return;
		}
		if (CurrentTab == UIInventoryFilter.ItemFilterType.QUEST)
		{
			ItemGrid.Locked = true;
			ItemGrid.InclusionFilter = null;
			ItemGrid.LoadInventory(GameState.s_playerCharacter.GetComponent<QuestInventory>());
			return;
		}
		ItemGrid.Locked = false;
		ItemGrid.InclusionFilter = (InventoryItem ii) => ii.baseItem.FilterType == CurrentTab || (CurrentTab == UIInventoryFilter.ItemFilterType.MISC && (ii.baseItem.FilterType == UIInventoryFilter.ItemFilterType.INGREDIENTS || ii.baseItem.FilterType == UIInventoryFilter.ItemFilterType.QUEST));
		ItemGrid.LoadInventory(GameState.s_playerCharacter.GetComponent<StashInventory>());
	}
}
