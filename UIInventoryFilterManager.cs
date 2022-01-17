using System;

public static class UIInventoryFilterManager
{
	public delegate void OnFiltersChanged(UIInventoryFilter.ItemFilterType mask);

	public static OnFiltersChanged FilterChanged;

	private static UIInventoryFilter.ItemFilterType m_FilterMask;

	private static Func<Item, bool> m_AdditionalFilter;

	public static Func<Item, bool> AdditionalFilter
	{
		get
		{
			return m_AdditionalFilter;
		}
		set
		{
			m_AdditionalFilter = value;
			if (FilterChanged != null)
			{
				FilterChanged(GetFilterMask());
			}
		}
	}

	public static string GetFilterTypeString(UIInventoryFilter.ItemFilterType ftype)
	{
		return ftype switch
		{
			UIInventoryFilter.ItemFilterType.ARMOR => GUIUtils.GetText(1006), 
			UIInventoryFilter.ItemFilterType.CLOTHING => GUIUtils.GetText(1426), 
			UIInventoryFilter.ItemFilterType.CONSUMABLES => GUIUtils.GetText(1427), 
			UIInventoryFilter.ItemFilterType.MISC => GUIUtils.GetText(1428), 
			UIInventoryFilter.ItemFilterType.WEAPONS => GUIUtils.GetText(1429), 
			UIInventoryFilter.ItemFilterType.INGREDIENTS => GUIUtils.GetText(1021), 
			UIInventoryFilter.ItemFilterType.QUEST => GUIUtils.GetText(993), 
			_ => "", 
		};
	}

	public static void ClearFilters()
	{
		m_FilterMask = UIInventoryFilter.ItemFilterType.NONE;
		AdditionalFilter = null;
	}

	public static bool GetFilter(UIInventoryFilter.ItemFilterType type)
	{
		return (m_FilterMask & type) != 0;
	}

	public static void SetFilter(UIInventoryFilter.ItemFilterType type, bool state)
	{
		if (m_FilterMask == type)
		{
			m_FilterMask = UIInventoryFilter.ItemFilterType.NONE;
		}
		else
		{
			m_FilterMask = type;
		}
		NotifyFilterChanged();
	}

	public static UIInventoryFilter.ItemFilterType GetFilterMask()
	{
		UIInventoryFilter.ItemFilterType itemFilterType = m_FilterMask;
		if (itemFilterType == UIInventoryFilter.ItemFilterType.NONE)
		{
			itemFilterType = UIInventoryFilter.ItemFilterType.WEAPONS | UIInventoryFilter.ItemFilterType.ARMOR | UIInventoryFilter.ItemFilterType.DEPRECATED_AMMO | UIInventoryFilter.ItemFilterType.CLOTHING | UIInventoryFilter.ItemFilterType.CONSUMABLES | UIInventoryFilter.ItemFilterType.INGREDIENTS | UIInventoryFilter.ItemFilterType.QUEST | UIInventoryFilter.ItemFilterType.MISC;
		}
		return itemFilterType;
	}

	public static bool Accepts(Item i)
	{
		if (!i)
		{
			return true;
		}
		if (AdditionalFilter != null && !AdditionalFilter(i))
		{
			return false;
		}
		if (i.FilterType == UIInventoryFilter.ItemFilterType.NONE && GetFilterMask() == (UIInventoryFilter.ItemFilterType.WEAPONS | UIInventoryFilter.ItemFilterType.ARMOR | UIInventoryFilter.ItemFilterType.DEPRECATED_AMMO | UIInventoryFilter.ItemFilterType.CLOTHING | UIInventoryFilter.ItemFilterType.CONSUMABLES | UIInventoryFilter.ItemFilterType.INGREDIENTS | UIInventoryFilter.ItemFilterType.QUEST | UIInventoryFilter.ItemFilterType.MISC))
		{
			return true;
		}
		return (i.FilterType & GetFilterMask()) > UIInventoryFilter.ItemFilterType.NONE;
	}

	private static void NotifyFilterChanged()
	{
		if (FilterChanged != null)
		{
			FilterChanged(GetFilterMask());
		}
	}
}
