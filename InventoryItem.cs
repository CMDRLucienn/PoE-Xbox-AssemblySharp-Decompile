using System;
using Polenter.Serialization;

[Serializable]
public class InventoryItem
{
	[ExcludeFromSerialization]
	public Item baseItem;

	public int stackSize = 1;

	public Item BaseItem
	{
		get
		{
			return baseItem;
		}
		set
		{
			baseItem = value;
		}
	}

	public int uiSlot { get; set; }

	public int StackSize
	{
		get
		{
			return stackSize;
		}
		set
		{
			stackSize = value;
		}
	}

	public bool Original { get; set; }

	public bool AreaLootSource { get; set; }

	public float GetBuyValue(Store store)
	{
		return baseItem.GetBuyValue(store) * (float)stackSize;
	}

	public float GetSellValue(Store store)
	{
		return baseItem.GetSellValue(store) * (float)stackSize;
	}

	public InventoryItem()
	{
		uiSlot = -1;
	}

	public InventoryItem(Item baseItem)
		: this()
	{
		this.baseItem = baseItem;
	}

	public InventoryItem(Item baseItem, int stackSize)
		: this(baseItem)
	{
		this.stackSize = stackSize;
	}

	public static bool ItemsCanStack(Item a, Item b)
	{
		if (!a.IsSameItem(b))
		{
			return false;
		}
		Equippable obj = a as Equippable;
		Equippable equippable = b as Equippable;
		if ((bool)obj || (bool)equippable)
		{
			return false;
		}
		return true;
	}

	public bool Equals(Item item)
	{
		if (item == null || baseItem == null)
		{
			return item == baseItem;
		}
		Item prefab = baseItem.Prefab;
		Item prefab2 = item.Prefab;
		if (!prefab2)
		{
			UIDebug.Instance.LogOnceOnlyWarning("Item '" + prefab2.name + "' does not have a prefab reference.", UIDebug.Department.Programming, 10f);
			return false;
		}
		if (!prefab)
		{
			UIDebug.Instance.LogOnceOnlyWarning("Item '" + baseItem.name + "' does not have a prefab reference.", UIDebug.Department.Programming, 10f);
			return false;
		}
		return prefab.IsSameItem(prefab2);
	}

	public bool NameEquals(string itemName)
	{
		if (string.IsNullOrEmpty(itemName) || baseItem == null || string.IsNullOrEmpty(baseItem.name))
		{
			return false;
		}
		return itemName.ToLower() == baseItem.name.Replace("(Clone)", "").ToLower();
	}

	public static int CompareSlots(InventoryItem x, InventoryItem y)
	{
		return x.uiSlot.CompareTo(y.uiSlot);
	}

	public void Copy(InventoryItem invenItem)
	{
		baseItem = invenItem.baseItem;
		stackSize = invenItem.stackSize;
	}

	public void SetStackSize(int newSize)
	{
		if ((bool)baseItem && (bool)baseItem.StoredInventory)
		{
			if (stackSize > newSize)
			{
				baseItem.StoredInventory.NotifyItemRemoved(baseItem, stackSize - newSize, Original);
			}
			else if (stackSize < newSize)
			{
				baseItem.StoredInventory.NotifyItemAdded(baseItem, newSize - stackSize);
			}
		}
		stackSize = newSize;
	}

	public override string ToString()
	{
		return "{" + stackSize + "x " + baseItem.name + "}";
	}

	public bool Empty()
	{
		if (!(baseItem == null))
		{
			return stackSize <= 0;
		}
		return true;
	}

	public static bool IsNullOrEmpty(InventoryItem item)
	{
		return item?.Empty() ?? true;
	}
}
