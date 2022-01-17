using System;
using System.ComponentModel;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;

[TypeConverter(typeof(PrefabConverter<Item>))]
public class Item : MonoBehaviour, ITooltipContent
{
	public enum UIDragDropSoundType
	{
		Armor_Plate,
		Armor_Chain,
		Armor_Leather,
		Armor_Cloth,
		Consumable_Potion,
		Consumable_Food,
		Consumable_Drugs,
		Loot_Scroll,
		Loot_Book,
		Loot_Generic_Jewelry,
		Loot_Keys,
		Loot_Gems,
		Loot_CreatureParts,
		Loot_Generic,
		Special_CampingSupplies,
		Special_LockPick,
		Special_GrapplingHookRope,
		Special_Torches,
		Weapon_SwordSmall,
		Weapon_SwordLarge,
		Weapon_StaffClub,
		Weapon_Wand,
		Weapon_Bow,
		Weapon_Crossbow,
		Weapon_Guns,
		Weapon_Mace,
		Weapon_Flail,
		Weapon_Dagger,
		Weapon_Shield,
		Loot_Ring,
		Loot_Necklace,
		Loot_Figurine,
		Consumable_Drink,
		Abydon_Hammer
	}

	public enum UIEquipSoundType
	{
		Default,
		Armor_Plate,
		Armor_Chain,
		Armor_Leather,
		Armor_Cloth,
		Weapon_SwordSmall,
		Weapon_SwordLarge,
		Weapon_StaffClub,
		Weapon_Wand,
		Weapon_Bow,
		Weapon_Crossbow,
		Weapon_Guns,
		Weapon_Mace,
		Weapon_Flail,
		Jewelry_Necklace,
		Jewelry_Rings,
		Misc_Grimoire,
		Misc_Pet,
		Weapon_Dagger,
		Weapon_Shield,
		Abydon_Hammer
	}

	public enum ItemLocation
	{
		Stored,
		Equipped,
		Prefab,
		Dragged
	}

	public class UIEquippedItem : ITooltipContent
	{
		public Item item;

		public UIEquippedItem(Item item)
		{
			this.item = item;
		}

		public virtual string GetTooltipContent(GameObject owner)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("[9AABFF](" + GUIUtils.GetText(1648) + ")[-]");
			stringBuilder.AppendLine(UIItemInspectManager.GetEquippableItemType(item, owner, item.GetComponent<Equippable>()).Trim());
			stringBuilder.AppendLine(item.GetString(owner).Trim());
			return stringBuilder.ToString();
		}

		public string GetTooltipName(GameObject owner)
		{
			return item.GetTooltipName(owner);
		}

		public Texture GetTooltipIcon()
		{
			return item.GetTooltipIcon();
		}
	}

	public class StoreItem : ITooltipContent
	{
		private Item Item;

		private Store Store;

		private bool Buy;

		public StoreItem(Item baseItem, Store store, bool buy)
		{
			Item = baseItem;
			Store = store;
			Buy = buy;
		}

		public string GetTooltipContent(GameObject owner)
		{
			return GUIUtils.Format(466, (Buy ? Item.GetBuyValue(Store) : Item.GetSellValue(Store)).ToString("#0")) + "\n" + Item.GetTooltipContent(owner);
		}

		public string GetTooltipName(GameObject owner)
		{
			return Item.GetTooltipName(owner);
		}

		public Texture GetTooltipIcon()
		{
			return Item.GetTooltipIcon();
		}
	}

	public UIInventoryFilter.ItemFilterType FilterType = UIInventoryFilter.ItemFilterType.MISC;

	[FormerlySerializedAs("InventoryDragDropSound")]
	public UIDragDropSoundType InventorySoundType = UIDragDropSoundType.Loot_Generic;

	public UIEquipSoundType InventoryEquipSound;

	[Tooltip("If this is set, the item is placed in the player's quest items inventory and can't be interacted with.")]
	public bool IsQuestItem;

	[Tooltip("If this is set, the item is stored in the player's ingredients inventory. These can be sold.")]
	public bool IsIngredient;

	[Tooltip("If this is set, stores buy this item from the player at its full value.")]
	public bool FullValueSell;

	public int MaxStackSize = 1;

	[Tooltip("If this is set, the loot system will never drop this item in a container or body bag")]
	public bool NeverDropAsLoot;

	[Tooltip("If this is set, the UI will identify this as a unique item.")]
	public bool Unique;

	public CurrencyValue Value = new CurrencyValue();

	public DatabaseString DisplayName = new DatabaseString(DatabaseString.StringTableType.Items);

	public DatabaseString DescriptionText = new DatabaseString(DatabaseString.StringTableType.Items);

	[Tooltip("Icon image used in inventory cells for the item.")]
	public Texture2D IconTexture;

	[Tooltip("Icon image used when dragging the item, and in the inspect window.")]
	public Texture2D IconLargeTexture;

	[Obsolete("Dropping items is not supported.")]
	public Container DroppedItemContainer;

	private ItemLocation m_location = ItemLocation.Prefab;

	private Item m_prefab;

	protected bool m_Initted;

	private bool m_shouldRender = true;

	protected Renderer[] m_renderers;

	private PE_DeferredPointLight[] m_lights;

	private Renderer m_Renderer;

	public bool IsRedirectIngredient
	{
		get
		{
			if (IsIngredient)
			{
				return !(this is Consumable);
			}
			return false;
		}
	}

	public string Name
	{
		get
		{
			if (this == null || base.gameObject == null)
			{
				return string.Empty;
			}
			Grimoire component = GetComponent<Grimoire>();
			if ((bool)component)
			{
				return StringUtility.Format(DisplayName, component.PrimaryOwnerName);
			}
			return DisplayName.GetText();
		}
	}

	public bool Renders
	{
		get
		{
			if ((bool)m_Renderer)
			{
				return m_Renderer.enabled;
			}
			return m_shouldRender;
		}
		set
		{
			if (m_renderers == null)
			{
				m_renderers = GetComponentsInChildren<Renderer>();
			}
			if (m_lights == null)
			{
				m_lights = GetComponentsInChildren<PE_DeferredPointLight>();
			}
			for (int i = 0; i < m_renderers.Length; i++)
			{
				if ((bool)m_renderers[i])
				{
					m_renderers[i].enabled = value;
				}
			}
			for (int j = 0; j < m_lights.Length; j++)
			{
				if ((bool)m_lights[j])
				{
					m_lights[j].enabled = value;
				}
			}
			m_shouldRender = value;
		}
	}

	public Item Prefab
	{
		get
		{
			if (m_prefab != null)
			{
				return m_prefab;
			}
			return this;
		}
		set
		{
			m_prefab = value;
		}
	}

	public bool IsPrefab => m_prefab == null;

	public BaseInventory StoredInventory { get; set; }

	public ItemLocation Location
	{
		get
		{
			return m_location;
		}
		set
		{
			Init();
			switch (value)
			{
			case ItemLocation.Stored:
				Renders = false;
				break;
			case ItemLocation.Equipped:
				Renders = true;
				break;
			}
			m_location = value;
		}
	}

	public virtual float GetValue()
	{
		return Value;
	}

	public float GetDefaultSellValue()
	{
		if (FullValueSell)
		{
			return Mathf.Floor(GetValue());
		}
		return Mathf.Floor(GetValue() * 0.2f);
	}

	public float GetDefaultBuyValue()
	{
		return Mathf.Floor(GetValue() * 1.5f);
	}

	public float GetSellValue(Store store)
	{
		if (FullValueSell)
		{
			return Mathf.Floor(GetValue());
		}
		return Mathf.Floor(GetValue() * store.buyMultiplier);
	}

	public float GetBuyValue(Store store)
	{
		return Mathf.Ceil(GetValue() * store.sellMultiplier);
	}

	public bool CanSell()
	{
		return !GetComponent<CanNotSell>();
	}

	public virtual void OnDestroy()
	{
		GameState.OnLevelLoaded -= GameState_OnLevelLoaded;
		ComponentUtils.NullOutObjectReferences(this);
	}

	public virtual void Awake()
	{
		GameState.OnLevelLoaded += GameState_OnLevelLoaded;
	}

	public virtual void Start()
	{
		Init();
		m_Renderer = GetComponent<Renderer>();
		bool flag2 = (Renders = PE_Paperdoll.IsObjectPaperdoll(base.gameObject));
		if (base.transform.parent == null && !GameState.LoadedGame && !GameState.IsRestoredLevel)
		{
			UIDebug.Instance.LogOnScreenWarning("Item: " + base.gameObject.name + " was found with no parent in scene. Item dropping no longer supported!", UIDebug.Department.Design, 10f);
		}
	}

	public virtual void Init()
	{
		m_Initted = true;
	}

	public void GameState_OnLevelLoaded(object sender, EventArgs e)
	{
		if (base.transform.parent == null)
		{
			GameUtilities.Destroy(base.gameObject);
		}
	}

	public virtual void Restored()
	{
		if (GameState.LoadedGame && m_prefab == null)
		{
			m_prefab = GameResources.LoadPrefab<Item>(base.name, instantiate: false);
			if (m_prefab == null)
			{
				m_prefab = this;
			}
		}
	}

	public Texture2D GetIconTexture()
	{
		EquipmentSoulbind component = GetComponent<EquipmentSoulbind>();
		if ((bool)component)
		{
			Texture2D overrideIconTexture = component.GetOverrideIconTexture();
			if ((bool)overrideIconTexture)
			{
				return overrideIconTexture;
			}
		}
		if ((bool)IconTexture)
		{
			return IconTexture;
		}
		return UIInventoryManager.Instance.DefaultItem;
	}

	public Texture2D GetIconLargeTexture()
	{
		EquipmentSoulbind component = GetComponent<EquipmentSoulbind>();
		if ((bool)component)
		{
			Texture2D overrideIconLargeTexture = component.GetOverrideIconLargeTexture();
			if ((bool)overrideIconLargeTexture)
			{
				return overrideIconLargeTexture;
			}
		}
		if ((bool)IconLargeTexture)
		{
			return IconLargeTexture;
		}
		Texture2D iconTexture = GetIconTexture();
		if ((bool)iconTexture && iconTexture != UIInventoryManager.Instance.DefaultItem)
		{
			return iconTexture;
		}
		return UIInventoryManager.Instance.DefaultItemLarge;
	}

	public bool NameEquals(string itemName)
	{
		if (string.IsNullOrEmpty(itemName) || string.IsNullOrEmpty(base.name))
		{
			return false;
		}
		return itemName.ToLower() == base.name.Replace("(Clone)", "").ToLower();
	}

	public bool IsSameItem(Item other)
	{
		if (DisplayName.StringTableID == other.DisplayName.StringTableID && DisplayName.StringID == other.DisplayName.StringID && DescriptionText.StringID == other.DescriptionText.StringID)
		{
			return DescriptionText.StringTableID == other.DescriptionText.StringTableID;
		}
		return false;
	}

	public virtual string GetString(GameObject owner)
	{
		return "";
	}

	public virtual string GetTooltipContent(GameObject owner)
	{
		string text = GetString(owner).Trim();
		text = text + "\n\n[" + NGUITools.EncodeColor(AttackBase.StringKeyColor) + "]" + GUIUtils.GetText(1796) + "[-]";
		return text.Trim();
	}

	public string GetTooltipName(GameObject owner)
	{
		return Name;
	}

	public Texture GetTooltipIcon()
	{
		return null;
	}
}
