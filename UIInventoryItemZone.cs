using UnityEngine;

public abstract class UIInventoryItemZone : MonoBehaviour
{
	public delegate void ItemSwapDelegate(Item baseItem, int quantity);

	public delegate void GridItemDelegate(UIInventoryGridItem grid);

	public ItemSwapDelegate OnItemTake;

	public ItemSwapDelegate OnItemPut;

	public bool Locked;

	public bool IsStore;

	public bool AllowMultiDrop = true;

	[Tooltip("Minimum access level at which items can be placed here.")]
	public AccessLevel RestrictExternalPlace = AccessLevel.InField;

	[Tooltip("Minimum access level at which items can be taken out of here.")]
	public AccessLevel RestrictExternalTake = AccessLevel.InField;

	[Tooltip("Minimum access level at which items can be moved between slots within a zone.")]
	public AccessLevel RestrictInternalMove = AccessLevel.InField;

	[Tooltip("If set, only the specified zones can accept items from here.")]
	public UIInventoryItemZone[] RestrictTransferTo;

	[Tooltip("Disallows the listed zones from recieving items from here.")]
	public UIInventoryItemZone[] ProhibitTransferTo;

	[Tooltip("If set, indicates that this zone represents a non-player and non-companion inventory.")]
	public bool IsExternalContainer;

	[Tooltip("If set, indicates that unsellable items should never go here.")]
	public bool IsSellBox;

	public GameObject OwnerGameObject { get; protected set; }

	public virtual bool IsQuickslotInventory => false;

	public virtual bool InfiniteStacking => false;

	public virtual bool CareAboutIndividualSlots => true;

	public static AccessLevel GetCurrentAccessLevel()
	{
		if (GameState.InCombat)
		{
			if (!IEModOptions.UnlockCombatInv) // if the "unlock inv" isn't activated
			{
				return AccessLevel.InCombat;
			}
		}
		/*
		pre-2.0:
		if ((!RestZone.PartyInRestZone && !UIInventoryManager.Instance.StashAccess) && (((GameState.Instance.CurrentMap != null) && GameState.Instance.CurrentMap.CanCamp) && !GameState.Option.GetOption(GameOption.BoolOption.DONT_RESTRICT_STASH)))
		{
			return AccessLevel.InField;
		}
		return AccessLevel.Rest;
		*/
		if (RestZone.PartyInRestZone || UIInventoryManager.Instance.StashAccess || GameState.Instance.CurrentMap == null || GameState.Instance.CurrentMap.CanAccessStash || GameState.Option.GetOption(GameOption.BoolOption.DONT_RESTRICT_STASH))
		{
			return AccessLevel.Rest;
		}
		return AccessLevel.InField;
	}

	protected virtual void OnDestroy()
	{
		ComponentUtils.NullOutObjectReferences(this);
	}

	public InventoryItem Take(InventoryItem from)
	{
		InventoryItem inventoryItem = DoTake(from);
		if (inventoryItem != null && OnItemTake != null)
		{
			OnItemTake(inventoryItem.baseItem, inventoryItem.stackSize);
		}
		return inventoryItem;
	}

	protected abstract InventoryItem DoTake(InventoryItem from);

	public bool CanPut(InventoryItem item, UIInventoryGridItem where)
	{
		return DoCanPut(item, where);
	}

	protected abstract bool DoCanPut(InventoryItem item, UIInventoryGridItem where);

	public bool Put(InventoryItem item, UIInventoryGridItem where)
	{
		if (OnItemPut != null)
		{
			OnItemPut(item.baseItem, item.stackSize);
		}
		return DoPut(item, where);
	}

	protected abstract bool DoPut(InventoryItem item, UIInventoryGridItem where);

	public Item Remove(Item item, int quantity)
	{
		Item item2 = DoRemove(item, quantity);
		if (item2 != null && OnItemTake != null)
		{
			OnItemTake(item2, quantity);
		}
		return item2;
	}

	protected abstract Item DoRemove(Item item, int quantity);

	public int Add(Item item, int quantity, UIInventoryGridItem where)
	{
		int num = DoAdd(item, quantity, where);
		if (quantity - num > 0 && OnItemPut != null)
		{
			OnItemPut(item, quantity - num);
		}
		return num;
	}

	protected abstract int DoAdd(Item item, int quantity, UIInventoryGridItem where);

	public abstract void Reload();
}
