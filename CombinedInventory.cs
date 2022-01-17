using System.Collections.Generic;
using UnityEngine;

public class CombinedInventory : Inventory
{
	private Dictionary<InventoryItem, Inventory> m_InventoryLookup = new Dictionary<InventoryItem, Inventory>();

	private Inventory m_DefaultInventory;

	private string m_GroupName;

	public string GroupName => m_GroupName;

	public List<Inventory> CombinedInventories
	{
		get
		{
			List<Inventory> list = new List<Inventory>();
			foreach (Inventory value in m_InventoryLookup.Values)
			{
				if (!list.Contains(value))
				{
					list.Add(value);
				}
			}
			return list;
		}
	}

	private void OnDestroy()
	{
		if (m_InventoryLookup != null)
		{
			m_InventoryLookup.Clear();
		}
		m_InventoryLookup = null;
		ComponentUtils.NullOutObjectReferences(this);
	}

	public void Initialize(string groupName, Inventory[] inventoriesToCombine)
	{
		m_GroupName = groupName;
		Inventory inventory = null;
		InventoryItem inventoryItem = null;
		int num = 0;
		for (int i = 0; i < inventoriesToCombine.Length; i++)
		{
			inventory = inventoriesToCombine[i];
			if (!(inventory != null))
			{
				continue;
			}
			if (m_DefaultInventory == null)
			{
				m_DefaultInventory = inventory;
			}
			if (inventory.ItemList.Count == 0)
			{
				m_InventoryLookup.Add(new InventoryItem(), inventory);
				continue;
			}
			for (int j = 0; j < inventory.ItemList.Count; j++)
			{
				inventoryItem = inventory.ItemList[j];
				if (inventoryItem == null || inventoryItem.baseItem == null)
				{
					Debug.LogWarning("CombinedInventory.Initialize() - Found null inventory item");
					continue;
				}
				if (m_InventoryLookup.ContainsKey(inventoryItem))
				{
					Debug.LogWarning(string.Concat("Unable to combine inventory. Inventory Item: ", inventoryItem, " is already in the combined list"));
					continue;
				}
				m_InventoryLookup.Add(inventoryItem, inventory);
				if (base.ItemList.Count >= MaxItems)
				{
					MaxItems += 10;
				}
				base.PutItem(inventoryItem, num++);
			}
		}
	}

	public override int AddItem(Item newItem, int addCount, int forceSlot, bool original)
	{
		HashSet<InventoryItem> other = new HashSet<InventoryItem>(m_DefaultInventory.ItemList);
		int result = m_DefaultInventory.AddItem(newItem, addCount, forceSlot, original);
		HashSet<InventoryItem> hashSet = new HashSet<InventoryItem>(m_DefaultInventory.ItemList);
		hashSet.ExceptWith(other);
		foreach (InventoryItem item in hashSet)
		{
			m_InventoryLookup.Add(item, m_DefaultInventory);
			base.PutItem(item, base.ItemList.Count);
		}
		return result;
	}

	public override int DestroyItem(Item item, int destroyCount)
	{
		Inventory inventory = FindInventoryForItem(item);
		if (inventory == null)
		{
			Debug.LogWarning("CombinedInventory.DestroyItem - Unable to find inventory to take item: " + item);
			inventory = m_DefaultInventory;
		}
		if (inventory != null)
		{
			inventory.DestroyItem(item, destroyCount);
		}
		return base.DestroyItem(item, destroyCount);
	}

	public override InventoryItem TakeItem(InventoryItem item)
	{
		Inventory inventory = FindInventoryForInventoryItem(item);
		if (inventory == null)
		{
			Debug.LogWarning("CombinedInventory.TakeItem - Unable to find inventory to take item: " + item);
			inventory = m_DefaultInventory;
		}
		base.TakeItem(item);
		return inventory.TakeItem(item);
	}

	public override bool PutItem(InventoryItem item, int slot)
	{
		Inventory inventory = FindInventoryForInventoryItem(item);
		if (inventory == null)
		{
			foreach (Inventory value in m_InventoryLookup.Values)
			{
				if (value.CanPutItem(item))
				{
					value.PutItem(item);
					m_InventoryLookup.Add(item, value);
					inventory = value;
					break;
				}
			}
		}
		base.PutItem(item, slot);
		return inventory.PutItem(item);
	}

	public override Item RemoveItem(Item item, int removeCount)
	{
		Inventory inventory = FindInventoryForItem(item);
		if (inventory != null || inventory.RemoveItem(item, removeCount) == null)
		{
			Debug.LogWarning("CombinedInventory.RemoveItem - Unable to remove item: " + item);
			inventory = m_DefaultInventory;
		}
		base.RemoveItem(item, removeCount);
		return inventory.RemoveItem(item, removeCount);
	}

	protected override void SetItemStored(Item baseItem)
	{
	}

	private Inventory FindInventoryForInventoryItem(InventoryItem itemToFind)
	{
		if (m_InventoryLookup.ContainsKey(itemToFind))
		{
			return m_InventoryLookup[itemToFind];
		}
		return null;
	}

	private Inventory FindInventoryForItem(Item itemToFind)
	{
		foreach (InventoryItem key in m_InventoryLookup.Keys)
		{
			if (key != null && key.baseItem.Equals(itemToFind))
			{
				return m_InventoryLookup[key];
			}
		}
		return null;
	}
}
