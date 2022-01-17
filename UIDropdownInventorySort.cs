using System;
using System.Collections.Generic;
using UnityEngine;

public class UIDropdownInventorySort : MonoBehaviour
{
	public enum InventorySort
	{
		None,
		ItemType,
		NameDesc,
		SellValueAsc,
		SellValueDesc,
		EnchanntmentAsc,
		EnchantmentDesc
	}

	public class InventorySortChoice
	{
		private InventorySort m_sortID;

		private Comparison<InventoryItem> m_ComparisonFunc;

		private int m_GuiStringID;

		public InventorySort ID => m_sortID;

		public Comparison<InventoryItem> SortFunc => m_ComparisonFunc;

		public InventorySortChoice(InventorySort sortID, int nameID, Comparison<InventoryItem> sortFunc)
		{
			m_sortID = sortID;
			m_GuiStringID = nameID;
			m_ComparisonFunc = sortFunc;
		}

		public override string ToString()
		{
			return GUIUtils.GetText(m_GuiStringID);
		}
	}

	public UIDropdownMenu Target;

	public UIInventoryStashGrid StashGrid;

	private static InventorySortChoice[] s_SortOptions;

	private void Awake()
	{
		if (StashGrid == null)
		{
			StashGrid = NGUITools.FindInParents<UIInventoryStashGrid>(base.gameObject);
		}
		if (Target == null)
		{
			Target = GetComponentInChildren<UIDropdownMenu>();
		}
		if (s_SortOptions == null)
		{
			Initialize();
		}
	}

	private void Start()
	{
		if (Target != null && s_SortOptions != null && s_SortOptions.Length != 0)
		{
			Target.OnDropdownOptionChanged += OnSortCriteriaChanged;
			object[] array = (Target.Options = s_SortOptions);
			Target.SelectedItem = s_SortOptions[0];
		}
	}

	private void OnDestroy()
	{
		if (Target != null)
		{
			Target.OnDropdownOptionChanged -= OnSortCriteriaChanged;
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void OnSortCriteriaChanged(object val)
	{
		if (val is InventorySortChoice currentSorting)
		{
			StashGrid.CurrentSorting = currentSorting;
		}
	}

	private void Initialize()
	{
		s_SortOptions = new List<InventorySortChoice>
		{
			new InventorySortChoice(InventorySort.None, 343, null),
			new InventorySortChoice(InventorySort.SellValueAsc, 1991, BaseInventory.CompareItemsBySellValue),
			new InventorySortChoice(InventorySort.SellValueDesc, 1992, (InventoryItem a, InventoryItem b) => BaseInventory.CompareItemsBySellValue(b, a)),
			new InventorySortChoice(InventorySort.EnchanntmentAsc, 1993, BaseInventory.CompareItemsByEnchantment),
			new InventorySortChoice(InventorySort.EnchantmentDesc, 1994, (InventoryItem a, InventoryItem b) => BaseInventory.CompareItemsByEnchantment(b, a)),
			new InventorySortChoice(InventorySort.ItemType, 1989, BaseInventory.CompareItemsByItemType)
		}.ToArray();
	}
}
