using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BaseInventory : MonoBehaviour, IEnumerable<InventoryItem>, IEnumerable
{
	public delegate void ItemAddedEventHandler(BaseInventory sender, Item item, int qty);

	public delegate void ItemRemovedEventHandler(BaseInventory sender, Item item, int qty, bool original);

	public List<InventoryItem> Items = new List<InventoryItem>();

	[Persistent]
	public int MaxItems = 40;

	private List<InventoryItem> m_items = new List<InventoryItem>();

	private Action m_closeInventoryCB;

	[HideInInspector]
	public bool OverrideRedirectToPlayer;

	protected bool m_IsPlayer;

	protected PartyMemberAI m_partyMemberAi;

	private int m_inventoryLoaded;

	private bool m_hasInstantiatedItemList;

	protected static bool m_EnableInventoryVoiceCues;

	public virtual bool InfiniteStacking => false;

	public CharacterStats AttachedCharacter { get; set; }

	public bool IsPartyMember
	{
		get
		{
			if ((bool)m_partyMemberAi)
			{
				return m_partyMemberAi.IsActiveInParty;
			}
			return false;
		}
	}

	protected virtual bool m_RedirectToPlayer
	{
		get
		{
			if (!IsPartyMember || m_IsPlayer)
			{
				return OverrideRedirectToPlayer;
			}
			return true;
		}
	}

	[Persistent]
	public List<InventoryItem> ItemList
	{
		get
		{
			return m_items;
		}
		set
		{
			if (m_inventoryLoaded == 1)
			{
				int num = 0;
				foreach (InventoryItem item in value)
				{
					Item baseItem = m_items[num].BaseItem;
					m_items[num] = item;
					m_items[num].BaseItem = baseItem;
					num++;
				}
			}
			else
			{
				m_items = value;
			}
			m_inventoryLoaded++;
			m_hasInstantiatedItemList = true;
		}
	}

	[Persistent(Persistent.ConversionType.GUIDLink)]
	public List<Item> SerializedItemList
	{
		get
		{
			List<Item> list = new List<Item>();
			foreach (InventoryItem item in m_items)
			{
				list.Add(item.BaseItem);
			}
			return list;
		}
		set
		{
			if (m_inventoryLoaded >= 2)
			{
				return;
			}
			if (m_inventoryLoaded > 0)
			{
				int num = 0;
				foreach (InventoryItem item3 in ItemList)
				{
					if (value.Count <= num)
					{
						break;
					}
					if (value[num] == null && item3.BaseItem != null)
					{
						continue;
					}
					Item item = null;
					Item item2 = value[num];
					if (item3.BaseItem != null && !item3.BaseItem.IsPrefab && item3.BaseItem != item2)
					{
						PersistenceManager.RemoveObject(item3.BaseItem.GetComponent<Persistence>());
						GameUtilities.Destroy(item3.BaseItem.gameObject);
					}
					else
					{
						item = item3.BaseItem;
					}
					if (item2 != null)
					{
						if (item != null && item.IsPrefab)
						{
							item2.Prefab = item;
						}
						SetItemStored(item2);
					}
					item3.BaseItem = value[num];
					num++;
				}
			}
			else
			{
				List<InventoryItem> list = new List<InventoryItem>();
				foreach (Item item4 in value)
				{
					InventoryItem inventoryItem = new InventoryItem(item4);
					inventoryItem.baseItem.StoredInventory = this;
					list.Add(inventoryItem);
				}
				ItemList = list;
			}
			m_inventoryLoaded++;
			m_hasInstantiatedItemList = true;
		}
	}

	public Action CloseInventoryCB
	{
		set
		{
			m_closeInventoryCB = value;
		}
	}

	public bool IsFull => GetFreeSpace() <= 0;

	public event Action<BaseInventory> OnChanged;

	public event ItemAddedEventHandler OnAdded;

	public event ItemRemovedEventHandler OnRemoved;

	IEnumerator IEnumerable.GetEnumerator()
	{
		return m_items.GetEnumerator();
	}

	public IEnumerator<InventoryItem> GetEnumerator()
	{
		return m_items.GetEnumerator();
	}

	public static int CompareItemsForShop(InventoryItem a, InventoryItem b)
	{
		if (!a.baseItem)
		{
			return 1;
		}
		if (!b.baseItem)
		{
			return -1;
		}
		int num = a.baseItem.FilterType - b.baseItem.FilterType;
		if (num != 0)
		{
			return num;
		}
		if (a.baseItem is Consumable && b.baseItem is Consumable)
		{
			num = (a.baseItem as Consumable).Type - (b.baseItem as Consumable).Type;
			if (num != 0)
			{
				return num;
			}
		}
		Equippable equippable = a.baseItem as Equippable;
		Equippable equippable2 = b.baseItem as Equippable;
		if ((bool)equippable && (bool)equippable2)
		{
			bool flag = equippable.SecondaryWeaponSlot && !equippable.PrimaryWeaponSlot;
			bool flag2 = equippable2.SecondaryWeaponSlot && !equippable2.PrimaryWeaponSlot;
			if (flag && !flag2)
			{
				return 1;
			}
			if (flag2 && !flag)
			{
				return -1;
			}
		}
		num = (int)(b.baseItem.GetValue() - a.baseItem.GetValue());
		if (num != 0)
		{
			return num;
		}
		return string.Compare(a.baseItem.Name, b.baseItem.Name);
	}

	public static int CompareItemsByItemType(InventoryItem a, InventoryItem b)
	{
		if (!a.baseItem)
		{
			return 1;
		}
		if (!b.baseItem)
		{
			return -1;
		}
		int num = 0;
		Equippable equippable = a.baseItem as Equippable;
		Equippable equippable2 = b.baseItem as Equippable;
		if (!equippable && !equippable2)
		{
			return CompareItemsForShop(a, b);
		}
		if (!equippable)
		{
			return 1;
		}
		if (!equippable2)
		{
			return -1;
		}
		num = UIItemInspectManager.GetEquippableItemTypeWithRarity(equippable, null, equippable, showUnique: false).CompareTo(UIItemInspectManager.GetEquippableItemTypeWithRarity(equippable2, null, equippable2, showUnique: false));
		if (num != 0)
		{
			return num;
		}
		return CompareItemsForShop(a, b);
	}

	public static int CompareItemsByEnchantment(InventoryItem a, InventoryItem b)
	{
		if (!a.baseItem)
		{
			return 1;
		}
		if (!b.baseItem)
		{
			return -1;
		}
		Equippable equippable = a.baseItem as Equippable;
		Equippable equippable2 = b.baseItem as Equippable;
		if (!equippable)
		{
			return 1;
		}
		if (!equippable2)
		{
			return -1;
		}
		int num = equippable.TotalItemModValue().CompareTo(equippable2.TotalItemModValue());
		if (num != 0)
		{
			return num;
		}
		return CompareItemsForShop(a, b);
	}

	public static int CompareItemsBySellValue(InventoryItem a, InventoryItem b)
	{
		if (!a.baseItem)
		{
			return 1;
		}
		if (!b.baseItem)
		{
			return -1;
		}
		int num = a.baseItem.GetDefaultSellValue().CompareTo(b.baseItem.GetDefaultSellValue());
		if (num != 0)
		{
			return num;
		}
		return CompareItemsForShop(a, b);
	}

	public void Sort(Comparison<InventoryItem> comparison)
	{
		m_items.Sort(comparison);
		for (int i = 0; i < m_items.Count; i++)
		{
			m_items[i].uiSlot = i;
		}
	}

	protected Item InstantiateStoredItem(Item prefab)
	{
		Item item = GameResources.Instantiate<Item>(prefab);
		item.Prefab = prefab;
		SetItemStored(item);
		return item;
	}

	public void persistItem(Item item)
	{
		Persistence component = GetComponent<Persistence>();
		if (component != null && !component.UnloadsBetweenLevels)
		{
			GameState.PersistAcrossSceneLoadsTracked(item);
			Persistence component2 = item.GetComponent<Persistence>();
			if ((bool)component2)
			{
				component2.UnloadsBetweenLevels = false;
				component2.SaveObject();
			}
		}
	}

	public bool Empty()
	{
		return m_items.Count == 0;
	}

	public void ClearInventory(bool deleteItems)
	{
		while (m_items.Count > 0)
		{
			InventoryItem inventoryItem = m_items[0];
			m_items.Remove(inventoryItem);
			if (deleteItems)
			{
				PersistenceManager.RemoveObject(inventoryItem.baseItem.gameObject.GetComponent<Persistence>());
				GameUtilities.Destroy(inventoryItem.baseItem.gameObject);
			}
		}
	}

	public int FirstFreeSlot()
	{
		for (int i = 0; i < MaxItems; i++)
		{
			bool flag = false;
			for (int j = 0; j < m_items.Count; j++)
			{
				if (m_items[j].uiSlot == i)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return i;
			}
		}
		return -1;
	}

	public int GetFreeSpace()
	{
		return MaxItems - ItemList.Count;
	}

	public void CompressSlots()
	{
		for (int num = m_items.Count - 1; num >= 0; num--)
		{
			if (InventoryItem.IsNullOrEmpty(m_items[num]))
			{
				m_items.RemoveAt(num);
			}
		}
		m_items.Sort(InventoryItem.CompareSlots);
		for (int i = 0; i < m_items.Count; i++)
		{
			m_items[i].uiSlot = i;
		}
	}

	public int GetSlotFor(Item item)
	{
		IEnumerable<InventoryItem> source = m_items.Where((InventoryItem it) => it.baseItem == item);
		if (source.Any())
		{
			return source.First().uiSlot;
		}
		return -1;
	}

	public virtual void Start()
	{
		OnRemoved += OnSelfItemRemoved;
		OnAdded += OnSelfItemAdded;
		m_partyMemberAi = GetComponent<PartyMemberAI>();
		m_IsPlayer = GetComponent<Player>();
		if (MaxItems < 0)
		{
			MaxItems = int.MaxValue;
		}
		if (Items.Count > MaxItems)
		{
			Debug.LogWarning("Inventory is over MaxItems.", this);
		}
	}

	public virtual void Restored()
	{
		if (m_items == null)
		{
			m_items = new List<InventoryItem>();
		}
		if (!m_hasInstantiatedItemList)
		{
			foreach (InventoryItem item in Items)
			{
				if (AddItem(item.baseItem, item.stackSize, -1, original: true) > 0)
				{
					Debug.LogError("Tried to initialize inventory of '" + base.gameObject.name + "' with item '" + item.baseItem.name + "' but there was no room.");
				}
			}
			m_hasInstantiatedItemList = true;
		}
		for (int i = 0; i < ItemList.Count; i++)
		{
			for (int num = ItemList.Count - 1; num >= i + 1; num--)
			{
				if (ItemList[i].baseItem == ItemList[num].baseItem)
				{
					ItemList.RemoveAt(num);
				}
			}
		}
	}

	protected virtual void Update()
	{
		if ((bool)AttachedCharacter)
		{
			MaxItems = AttachedCharacter.InventoryMaxSize;
		}
	}

	private void OnSelfItemRemoved(BaseInventory sender, Item item, int removedQty, bool original)
	{
		NotifyChanged();
	}

	private void OnSelfItemAdded(BaseInventory sender, Item item, int addedQty)
	{
		NotifyChanged();
	}

	public void NotifyItemRemoved(InventoryItem item)
	{
		NotifyItemRemoved(item.baseItem, item.stackSize, item.Original);
	}

	public void NotifyItemRemoved(Item item, int removeQty, bool original)
	{
		if (!(item == null) && this.OnRemoved != null)
		{
			this.OnRemoved(this, item, removeQty, original);
		}
	}

	public void NotifyItemAdded(InventoryItem item)
	{
		NotifyItemAdded(item.baseItem, item.stackSize);
	}

	public void NotifyItemAdded(Item item, int addedQty)
	{
		if (!(item == null) && this.OnAdded != null)
		{
			this.OnAdded(this, item, addedQty);
		}
	}

	private void NotifyChanged()
	{
		if (this.OnChanged != null)
		{
			this.OnChanged(this);
		}
	}

	protected void RemoveEmpty()
	{
		for (int num = ItemList.Count - 1; num >= 0; num--)
		{
			if (ItemList[num] == null || ItemList[num].baseItem == null)
			{
				ItemList.RemoveAt(num);
			}
		}
	}

	public void CloseInventory()
	{
		if (m_closeInventoryCB != null)
		{
			m_closeInventoryCB();
		}
	}

	private bool TryRedirectToPlayer(Item newItem, int addCount, int forceSlot, ref int qty)
	{
		if (m_RedirectToPlayer && IsRedirectItem(newItem))
		{
			qty = GameState.s_playerCharacter.Inventory.AddItem(newItem, addCount, forceSlot);
			return true;
		}
		return false;
	}

	private bool DoesRedirectToPlayer(InventoryItem item)
	{
		if (m_RedirectToPlayer)
		{
			return IsRedirectItem(item.baseItem);
		}
		return false;
	}

	private bool TryRedirectToPlayer(InventoryItem item)
	{
		if (DoesRedirectToPlayer(item))
		{
			return GameState.s_playerCharacter.Inventory.PutItem(item);
		}
		return false;
	}

	private bool CanRedirectToPlayer(InventoryItem item)
	{
		if (DoesRedirectToPlayer(item))
		{
			return GameState.s_playerCharacter.Inventory.CanPutItem(item);
		}
		return false;
	}

	protected bool IsRedirectItem(Item newItem)
	{
		if (!(newItem is Currency) && !(newItem is CampingSupplies) && !newItem.IsQuestItem)
		{
			return newItem.IsRedirectIngredient;
		}
		return true;
	}

	public int AddItem(Item newItem, int addCount)
	{
		return AddItem(newItem, addCount, -1, original: false);
	}

	public int AddItem(Item newItem, int addCount, int forceSlot)
	{
		return AddItem(newItem, addCount, forceSlot, original: false);
	}

	public virtual int AddItem(Item newItem, int addCount, int forceSlot, bool original)
	{
		int num = addCount;
		if (addCount == 0)
		{
			return 0;
		}
		if (newItem == null)
		{
			return 0;
		}
		int qty = 0;
		if (TryRedirectToPlayer(newItem, addCount, forceSlot, ref qty))
		{
			return qty;
		}
		RemoveEmpty();
		int num2 = newItem.MaxStackSize;
		if (InfiniteStacking && InventoryItem.ItemsCanStack(newItem, newItem))
		{
			num2 = int.MaxValue;
		}
		if (num2 > 1 && forceSlot < 0)
		{
			foreach (InventoryItem item in ItemList)
			{
				if (!InventoryItem.ItemsCanStack(item.baseItem, newItem) || item.stackSize >= num2)
				{
					continue;
				}
				item.stackSize += addCount;
				item.Original |= original;
				if (item.stackSize >= num2)
				{
					NotifyItemAdded(item.baseItem, addCount);
					addCount = item.stackSize - num2;
					item.stackSize = num2;
					continue;
				}
				NotifyItemAdded(item.baseItem, addCount);
				addCount = 0;
				if (!newItem.IsPrefab)
				{
					Persistence component = newItem.GetComponent<Persistence>();
					if ((bool)component)
					{
						component.SetForDestroy();
					}
					GameUtilities.Destroy(newItem.gameObject);
				}
				return 0;
			}
		}
		while (ItemList.Count < MaxItems && addCount > 0)
		{
			InventoryItem inventoryItem = new InventoryItem();
			inventoryItem.stackSize = addCount;
			inventoryItem.Original = original;
			if (inventoryItem.stackSize > num2)
			{
				addCount = inventoryItem.stackSize - num2;
				inventoryItem.stackSize = num2;
				inventoryItem.baseItem = InstantiateStoredItem(newItem.Prefab);
			}
			else
			{
				if (newItem.IsPrefab)
				{
					inventoryItem.baseItem = InstantiateStoredItem(newItem.Prefab);
				}
				else
				{
					inventoryItem.baseItem = newItem;
					SetItemStored(newItem);
				}
				addCount = 0;
			}
			persistItem(inventoryItem.baseItem);
			NotifyItemAdded(inventoryItem);
			if (forceSlot >= 0)
			{
				inventoryItem.uiSlot = forceSlot;
			}
			else
			{
				inventoryItem.uiSlot = FirstFreeSlot();
			}
			ItemList.Add(inventoryItem);
		}
		if (num != addCount)
		{
			CallObtainedScripts(newItem);
		}
		else if (m_EnableInventoryVoiceCues && (bool)m_partyMemberAi)
		{
			SoundSet.TryPlayVoiceEffectWithLocalCooldown(base.gameObject, SoundSet.SoundAction.InventoryFull, SoundSet.s_MediumVODelay, forceInterrupt: false);
		}
		return addCount;
	}

	public bool WillItemStackInInventory(Item itemToCheck)
	{
		if (!InfiniteStacking || !InventoryItem.ItemsCanStack(itemToCheck, itemToCheck))
		{
			return false;
		}
		return true;
	}

	public virtual Item RemoveItem(Item item, int removeCount)
	{
		if (removeCount <= 0)
		{
			return null;
		}
		List<InventoryItem> list = new List<InventoryItem>();
		foreach (InventoryItem item2 in ItemList)
		{
			if (item2.Equals(item))
			{
				int stackSize = item2.stackSize;
				item2.stackSize -= removeCount;
				if (item2.stackSize > 0)
				{
					NotifyItemRemoved(item2.baseItem, removeCount, item2.Original);
					removeCount = 0;
					break;
				}
				NotifyItemRemoved(item2.baseItem, stackSize, item2.Original);
				list.Add(item2);
				removeCount = Mathf.Abs(item2.stackSize);
				if (removeCount == 0)
				{
					break;
				}
			}
		}
		if (removeCount > 0)
		{
			Debug.LogWarning("Prevented attempt to remove more items than inventory contained.");
		}
		Item prefab = item.Prefab;
		foreach (InventoryItem item3 in list)
		{
			ItemList.Remove(item3);
			PersistenceManager.RemoveObject(item3.baseItem.GetComponent<Persistence>());
			GameUtilities.Destroy(item3.baseItem.gameObject);
		}
		return InstantiateStoredItem(prefab);
	}

	public Item RemoveItem(InventoryItem item)
	{
		return RemoveItem(item.baseItem, item.stackSize);
	}

	public virtual bool CanPutItem(InventoryItem item, bool isSwap = false)
	{
		if (item == null || item.baseItem == null)
		{
			return true;
		}
		if (DoesRedirectToPlayer(item))
		{
			return CanRedirectToPlayer(item);
		}
		RemoveEmpty();
		if (!isSwap && ItemList.Count >= MaxItems)
		{
			return false;
		}
		if (!InfiniteStacking && item.baseItem.MaxStackSize < item.stackSize)
		{
			return false;
		}
		return true;
	}

	public bool PutItem(InventoryItem item)
	{
		return PutItem(item, -1);
	}

	public virtual bool PutItem(InventoryItem item, int slot)
	{
		if (item == null || item.baseItem == null)
		{
			return true;
		}
		for (int i = 0; i < ItemList.Count; i++)
		{
			if (ItemList[i].baseItem == item.baseItem)
			{
				return true;
			}
		}
		if (DoesRedirectToPlayer(item))
		{
			return TryRedirectToPlayer(item);
		}
		RemoveEmpty();
		if (ItemList.Count >= MaxItems)
		{
			return false;
		}
		if (!InfiniteStacking && item.stackSize > item.baseItem.MaxStackSize)
		{
			return false;
		}
		if (m_RedirectToPlayer)
		{
			item.Original = false;
		}
		bool flag = false;
		if (InfiniteStacking)
		{
			for (int j = 0; j < ItemList.Count; j++)
			{
				if (InventoryItem.ItemsCanStack(ItemList[j].baseItem, item.baseItem))
				{
					flag = true;
					ItemList[j].stackSize += item.stackSize;
					Persistence component = item.baseItem.GetComponent<Persistence>();
					if ((bool)component)
					{
						component.SetForDestroy();
					}
					GameUtilities.Destroy(item.baseItem.gameObject);
					item = ItemList[j];
					break;
				}
			}
		}
		if (!flag)
		{
			SetItemStored(item.baseItem);
			persistItem(item.baseItem);
			ItemList.Add(item);
			item.uiSlot = slot;
		}
		NotifyItemAdded(item);
		CallObtainedScripts(item.baseItem);
		return true;
	}

	protected void CallObtainedScripts(Item newItem)
	{
		if (IsPartyMember)
		{
			ScriptEvent component = newItem.GetComponent<ScriptEvent>();
			if ((bool)component)
			{
				component.ExecuteScript(ScriptEvent.ScriptEvents.OnItemCollected);
			}
			string itemName = ((!newItem.Prefab) ? newItem.name.Replace("(Clone)", "") : newItem.Prefab.name);
			TutorialManager.TutorialTrigger trigger = new TutorialManager.TutorialTrigger(TutorialManager.TriggerType.ITEM_COLLECTED);
			trigger.ItemName = itemName;
			TutorialManager.STriggerTutorialsOfType(trigger);
			if ((bool)newItem.GetComponent<Grimoire>())
			{
				TutorialManager.STriggerTutorialsOfTypeFast(TutorialManager.ExclusiveTriggerType.GRIMOIRE_LOOTED);
			}
		}
	}

	public virtual InventoryItem TakeItem(InventoryItem item)
	{
		if (item == null || item.baseItem == null)
		{
			return null;
		}
		ItemList.Remove(item);
		NotifyItemRemoved(item);
		return item;
	}

	public virtual int DestroyItem(Item item, int destroyCount)
	{
		List<InventoryItem> list = new List<InventoryItem>();
		foreach (InventoryItem item2 in ItemList)
		{
			if (item2.Equals(item))
			{
				int stackSize = item2.stackSize;
				item2.stackSize -= destroyCount;
				if (item2.stackSize > 0)
				{
					NotifyItemRemoved(item2.baseItem, destroyCount, item2.Original);
					destroyCount = 0;
					break;
				}
				NotifyItemRemoved(item2.baseItem, stackSize, item2.Original);
				list.Add(item2);
				destroyCount = Mathf.Abs(item2.stackSize);
				if (destroyCount == 0)
				{
					break;
				}
			}
		}
		foreach (InventoryItem item3 in list)
		{
			ItemList.Remove(item3);
			PersistenceManager.RemoveObject(item3.BaseItem.GetComponent<Persistence>());
			GameUtilities.Destroy(item3.baseItem.gameObject);
		}
		return destroyCount;
	}

	public virtual int DestroyItem(string itemName, int destroyCount)
	{
		List<InventoryItem> list = new List<InventoryItem>();
		foreach (InventoryItem item in ItemList)
		{
			if (item.NameEquals(itemName))
			{
				int stackSize = item.stackSize;
				item.stackSize -= destroyCount;
				if (item.stackSize > 0)
				{
					NotifyItemRemoved(item.baseItem, destroyCount, item.Original);
					destroyCount = 0;
					break;
				}
				NotifyItemRemoved(item.baseItem, stackSize, item.Original);
				list.Add(item);
				destroyCount = Mathf.Abs(item.stackSize);
				if (destroyCount == 0)
				{
					break;
				}
			}
		}
		foreach (InventoryItem item2 in list)
		{
			ItemList.Remove(item2);
			PersistenceManager.RemoveObject(item2.BaseItem.GetComponent<Persistence>());
			GameUtilities.Destroy(item2.baseItem.gameObject);
		}
		return destroyCount;
	}

	public virtual int ItemCount(Item item)
	{
		int num = 0;
		for (int i = 0; i < ItemList.Count; i++)
		{
			if (ItemList[i] != null && ItemList[i].Equals(item))
			{
				num += ItemList[i].stackSize;
			}
		}
		return num;
	}

	public virtual int ItemCount(string itemName)
	{
		int num = 0;
		for (int i = 0; i < ItemList.Count; i++)
		{
			if (ItemList[i] != null && ItemList[i].NameEquals(itemName))
			{
				num += ItemList[i].stackSize;
			}
		}
		return num;
	}

	protected virtual void SetItemStored(Item baseItem)
	{
		baseItem.Location = Item.ItemLocation.Stored;
		baseItem.StoredInventory = this;
		baseItem.transform.parent = base.transform;
		baseItem.transform.localPosition = Vector3.zero;
		baseItem.transform.localRotation = Quaternion.identity;
		Equippable equippable = baseItem as Equippable;
		if ((bool)equippable)
		{
			equippable.RecacheTransforms();
		}
	}

	public void SetPartyMemberInventory(bool enabled)
	{
		m_partyMemberAi = GetComponent<PartyMemberAI>();
		m_IsPlayer = GetComponent<Player>();
		if (!enabled)
		{
			return;
		}
		for (int num = ItemList.Count - 1; num >= 0; num--)
		{
			InventoryItem inventoryItem = TakeItem(ItemList[num]);
			if (inventoryItem != null)
			{
				int uiSlot = inventoryItem.uiSlot;
				PutItem(inventoryItem);
				inventoryItem.uiSlot = uiSlot;
			}
		}
	}
}
