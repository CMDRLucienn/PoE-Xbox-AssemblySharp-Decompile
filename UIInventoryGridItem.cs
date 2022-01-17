using System;
using System.Linq;
using UnityEngine;

public class UIInventoryGridItem : MonoBehaviour
{
	public enum GlowType
	{
		NONE,
		ENCHANTED,
		UNIQUE,
		SOULBOUND
	}

	private const float FILTERED_ALPHA = 0.3f;

	private const float FADE_DURATION = 0.085f;

	private const float TOOLTIP_REPEAT_DELAY = 0.6f;

	private const float TOOLTIP_REPEAT_MAXDIST = 80f;

	private static float s_TooltipRepeatWindow = 0f;

	private static Vector2 s_TooltipRepeatLast = Vector2.zero;

	public UIInventoryGridItem WeaponSetBuddy;

	public UITexture Icon;

	public GameObject Background;

	public GameObject NonDragBackground;

	public UILabel QuantityLabel;

	private TweenColor m_BackgroundFadeOut;

	private UISprite m_BackgroundSprite;

	private UISprite m_HoveredSprite;

	private UISprite m_SelectedSprite;

	private UISprite m_MakeLinkSprite;

	private UISprite m_IsLinkedSprite;

	private UIDragPanelContents m_DragComponent;

	private UISprite m_EquipmentSlotSprite;

	private UISprite m_SpriteModGlow;

	public GameObject WidgetOwner;

	public bool HideWhenBlocked;

	private const string SpriteName_ModBG_Soulbound = "magicItemGlowSoulbound";

	private const string SpriteName_ModBG_Unique = "magicItemGlowUnique";

	private const string SpriteName_ModBG_Enchanted = "magicItemGlow";

	private bool m_IsLinked;

	private bool m_Hovered;

	private bool m_Tooltipped;

	private float m_SecondTooltipTimer;

	public UIInventoryItemZone ClickSendsTo;

	public bool ClickSendsToPlayer;

	public UIInventoryItemZone[] AllowGiveTo;

	public Equippable.EquipmentSlot EquipmentSlot = Equippable.EquipmentSlot.None;

	public UIInventoryFilter.ItemFilterType RestrictByFilter;

	public Equippable.EquipmentSlot OrAllowEquipment = Equippable.EquipmentSlot.None;

	private InventoryItem m_Item;

	private bool m_Selected;

	private UIInventoryItemZone m_Owner;

	private int m_Slot;

	private float m_IconAlpha = 1f;

	private Equippable m_ListenedEquippable;

	public UIInventoryItemZone.ItemSwapDelegate OnItemPut;

	public UIInventoryItemZone.ItemSwapDelegate OnItemTake;

	public UIInventoryItemZone.GridItemDelegate OnItemReload;

	public UIEventListener.VoidDelegate OnClicked;

	public UIEventListener.FloatDelegate OnScrolled;

	public bool Locked;

	private bool m_Blocked;

	public bool SwapIsPut;

	public bool IsEquipped;

	public bool TooltipDisabled;

	public bool FilterDisabled;

	private Vector2 ComparisonOffset = new Vector2(-6f, 0f);

	private bool m_Initted;

	private static UIMessageBox s_ActiveSplitDialog;

	private static bool s_BlockRefresh = false;

	private string m_OriginalBgSpriteName = "";

	private bool LinkAllowed => m_MakeLinkSprite != null;

	public InventoryItem InvItem => m_Item;

	public bool Empty => m_Item == null;

	public bool Selected
	{
		get
		{
			return m_Selected;
		}
		set
		{
			m_Selected = value;
			if ((bool)m_SelectedSprite)
			{
				m_SelectedSprite.alpha = (m_Selected ? 1 : 0);
			}
		}
	}

	public UIInventoryItemZone Owner => m_Owner;

	public int Slot => m_Slot;

	public bool Blocked => m_Blocked;

	public static string GetModBgSpriteName(GlowType type)
	{
		return type switch
		{
			GlowType.ENCHANTED => "magicItemGlow", 
			GlowType.UNIQUE => "magicItemGlowUnique", 
			GlowType.SOULBOUND => "magicItemGlowSoulbound", 
			_ => "", 
		};
	}

	private void OnEnable()
	{
		if ((bool)m_DragComponent)
		{
			m_DragComponent.enabled = true;
		}
		UIGlobalInventory.Instance.Activated(this);
	}

	private void OnDisable()
	{
		UIGlobalInventory.Instance.Deactivated(this);
		m_Hovered = false;
		RefreshColor();
	}

	private void Start()
	{
		Init();
		UIInventoryFilterManager.FilterChanged = (UIInventoryFilterManager.OnFiltersChanged)Delegate.Combine(UIInventoryFilterManager.FilterChanged, new UIInventoryFilterManager.OnFiltersChanged(OnFilterChanged));
		OnItemPut = (UIInventoryItemZone.ItemSwapDelegate)Delegate.Combine(OnItemPut, new UIInventoryItemZone.ItemSwapDelegate(SendReload));
		OnItemTake = (UIInventoryItemZone.ItemSwapDelegate)Delegate.Combine(OnItemTake, new UIInventoryItemZone.ItemSwapDelegate(SendReload));
		UIImageButtonRevised[] componentsInChildren = GetComponentsInChildren<UIImageButtonRevised>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].IsInspectable = true;
		}
	}

	private void OnDestroy()
	{
		UIInventoryFilterManager.FilterChanged = (UIInventoryFilterManager.OnFiltersChanged)Delegate.Remove(UIInventoryFilterManager.FilterChanged, new UIInventoryFilterManager.OnFiltersChanged(OnFilterChanged));
		UnsubscribeEvents();
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void SendReload(Item baseItem, int qty)
	{
		if (OnItemReload != null)
		{
			OnItemReload(this);
		}
	}

	private void OnFilterChanged(UIInventoryFilter.ItemFilterType mask)
	{
		RefreshFiltered();
	}

	public void Init(bool unrestricted = false)
	{
		if (m_Initted)
		{
			return;
		}
		if (m_Owner == null)
		{
			m_Owner = NGUITools.FindInParents<UIInventoryItemZone>(base.gameObject);
		}
		UIInventoryItemGrid uIInventoryItemGrid = m_Owner as UIInventoryItemGrid;
		if ((bool)uIInventoryItemGrid && uIInventoryItemGrid.RecycleWidgets && !unrestricted)
		{
			return;
		}
		m_Initted = true;
		if ((bool)Background)
		{
			Vector3 localPosition = Background.transform.localPosition;
			localPosition.z = 0f;
			Background.transform.localPosition = localPosition;
			m_BackgroundSprite = Background.GetComponent<UISprite>();
			localPosition = Icon.transform.localPosition;
			localPosition.z = -3f;
			Icon.transform.localPosition = localPosition;
			if ((bool)QuantityLabel)
			{
				localPosition = QuantityLabel.transform.localPosition;
				localPosition.z = -4f;
				QuantityLabel.transform.localPosition = localPosition;
			}
			m_DragComponent = Background.GetComponent<UIDragPanelContents>();
			Transform parent = (WidgetOwner ? WidgetOwner.transform : base.gameObject.transform);
			if (m_HoveredSprite == null)
			{
				Transform transform = base.transform.Find("Hovered");
				if ((bool)transform)
				{
					m_HoveredSprite = transform.GetComponent<UISprite>();
				}
			}
			if (m_HoveredSprite == null)
			{
				GameObject gameObject = new GameObject("Hovered");
				Transform obj = gameObject.transform;
				obj.parent = parent;
				obj.localPosition = Icon.transform.localPosition;
				obj.localScale = Icon.transform.localScale;
				obj.localRotation = Quaternion.identity;
				gameObject.layer = Background.gameObject.layer;
				m_HoveredSprite = gameObject.AddComponent<UISprite>();
				m_HoveredSprite.atlas = UIAtlasManager.Instance.CommonTinted;
				m_HoveredSprite.spriteName = "white";
				m_HoveredSprite.depth = 2;
				m_HoveredSprite.alpha = 0f;
			}
			if (m_EquipmentSlotSprite == null)
			{
				Transform transform2 = base.transform.Find("EquipmentIcon");
				if ((bool)transform2)
				{
					m_EquipmentSlotSprite = transform2.GetComponent<UISprite>();
				}
			}
			if (m_EquipmentSlotSprite == null && !string.IsNullOrEmpty(GetEquipmentSlotSprite(EquipmentSlot)))
			{
				GameObject gameObject2 = new GameObject("EquipmentIcon");
				Transform obj2 = gameObject2.transform;
				obj2.parent = parent;
				obj2.localPosition = Icon.transform.localPosition;
				obj2.localRotation = Quaternion.identity;
				gameObject2.layer = Background.gameObject.layer;
				m_EquipmentSlotSprite = gameObject2.AddComponent<UISprite>();
				m_EquipmentSlotSprite.atlas = UIAtlasManager.Instance.Inventory;
				m_EquipmentSlotSprite.spriteName = GetEquipmentSlotSprite(EquipmentSlot);
				m_EquipmentSlotSprite.MakePixelPerfect();
				m_EquipmentSlotSprite.depth = 2;
				m_EquipmentSlotSprite.alpha = 0f;
			}
			if (m_SelectedSprite == null)
			{
				Transform transform3 = base.transform.Find("Selected");
				if ((bool)transform3)
				{
					m_SelectedSprite = transform3.GetComponent<UISprite>();
				}
			}
			if (m_SelectedSprite == null)
			{
				GameObject gameObject3 = new GameObject("Selected");
				Transform obj3 = gameObject3.transform;
				obj3.parent = parent;
				obj3.localPosition = Icon.transform.localPosition;
				obj3.localRotation = Quaternion.identity;
				gameObject3.layer = Background.gameObject.layer;
				m_SelectedSprite = gameObject3.AddComponent<UISprite>();
				m_SelectedSprite.atlas = UIAtlasManager.Instance.InventoryTop;
				m_SelectedSprite.spriteName = "weaponSetSelected";
				m_SelectedSprite.color = Color.white;
				m_SelectedSprite.MakePixelPerfect();
				m_SelectedSprite.depth = 2;
				m_SelectedSprite.alpha = 0f;
			}
			if (m_BackgroundFadeOut == null && (bool)m_HoveredSprite)
			{
				m_BackgroundFadeOut = m_HoveredSprite.gameObject.AddComponent<TweenColor>();
				m_BackgroundFadeOut.to = UIInventoryManager.Instance.ItemNormalColor;
				m_BackgroundFadeOut.duration = 0.085f;
			}
		}
		SubscribeEvents();
		Selected = false;
	}

	public static GlowType DetermineEquipModGlowColor(Equippable equipItem)
	{
		if (equipItem == null)
		{
			return GlowType.NONE;
		}
		if ((bool)equipItem.GetComponent<EquipmentSoulbind>())
		{
			return GlowType.SOULBOUND;
		}
		if (equipItem.Unique)
		{
			return GlowType.UNIQUE;
		}
		if (equipItem.TotalItemModValue() > 0)
		{
			return GlowType.ENCHANTED;
		}
		return GlowType.NONE;
	}

	private void DetermineAndSetEquipModGlowColor(Equippable equipItem)
	{
		SetModGlow(DetermineEquipModGlowColor(equipItem));
	}

	private void SetModGlow(GlowType type)
	{
		if (!(m_SpriteModGlow == null) || type != 0)
		{
			if (!m_Initted)
			{
				Init();
			}
			if (m_SpriteModGlow == null && Icon != null)
			{
				m_SpriteModGlow = NGUITools.AddChild<UISprite>(WidgetOwner ? WidgetOwner : Icon.transform.parent.gameObject);
				m_SpriteModGlow.atlas = UIAtlasManager.Instance.Inventory;
				m_SpriteModGlow.color = Color.white;
				m_SpriteModGlow.transform.localPosition = Icon.transform.localPosition;
				m_SpriteModGlow.depth = ((m_BackgroundSprite != null) ? (m_BackgroundSprite.depth + 1) : 3);
			}
			if ((bool)m_SpriteModGlow)
			{
				m_SpriteModGlow.spriteName = GetModBgSpriteName(type);
				m_SpriteModGlow.MakePixelPerfect();
				m_SpriteModGlow.gameObject.SetActive(type != GlowType.NONE);
			}
		}
	}

	private string GetEquipmentSlotSprite(Equippable.EquipmentSlot slot)
	{
		switch (slot)
		{
		case Equippable.EquipmentSlot.Armor:
			return "ICO_invArmor";
		case Equippable.EquipmentSlot.Cape_DEPRECATED:
			return "ICO_invCloak";
		case Equippable.EquipmentSlot.Feet:
			return "ICO_invBoot";
		case Equippable.EquipmentSlot.Grimoire:
			return "ICO_invGrim";
		case Equippable.EquipmentSlot.Hands:
			return "ICO_invGauntlets";
		case Equippable.EquipmentSlot.Head:
			return "ICO_invHelm";
		case Equippable.EquipmentSlot.RightRing:
		case Equippable.EquipmentSlot.LeftRing:
			return "ICO_invRing";
		case Equippable.EquipmentSlot.Neck:
			return "ICO_invCloak";
		case Equippable.EquipmentSlot.Pet:
			return "ICO_invPet";
		case Equippable.EquipmentSlot.Waist:
			return "ICO_invBelt";
		default:
			return "";
		}
	}

	private void SubscribeEvents()
	{
		if ((bool)Background)
		{
			UIEventListener uIEventListener = UIEventListener.Get(Background);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnClick));
			uIEventListener.onRightClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onRightClick, new UIEventListener.VoidDelegate(OnRightClick));
			uIEventListener.onDoubleClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onDoubleClick, new UIEventListener.VoidDelegate(OnDoubleClick));
			uIEventListener.onScroll = (UIEventListener.FloatDelegate)Delegate.Combine(uIEventListener.onScroll, new UIEventListener.FloatDelegate(OnScroll));
			uIEventListener.onHover = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onHover, new UIEventListener.BoolDelegate(OnHover));
			uIEventListener.onTooltip = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onTooltip, new UIEventListener.BoolDelegate(OnTooltip));
			uIEventListener.onDrag = (UIEventListener.VectorDelegate)Delegate.Combine(uIEventListener.onDrag, new UIEventListener.VectorDelegate(OnDrag));
			uIEventListener.onDrop = (UIEventListener.ObjectDelegate)Delegate.Combine(uIEventListener.onDrop, new UIEventListener.ObjectDelegate(OnDrop));
		}
		if ((bool)NonDragBackground)
		{
			UIEventListener uIEventListener2 = UIEventListener.Get(NonDragBackground);
			uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, new UIEventListener.VoidDelegate(OnClick));
			uIEventListener2.onRightClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onRightClick, new UIEventListener.VoidDelegate(OnRightClick));
			uIEventListener2.onDoubleClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onDoubleClick, new UIEventListener.VoidDelegate(OnDoubleClick));
			uIEventListener2.onScroll = (UIEventListener.FloatDelegate)Delegate.Combine(uIEventListener2.onScroll, new UIEventListener.FloatDelegate(OnScroll));
			uIEventListener2.onHover = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener2.onHover, new UIEventListener.BoolDelegate(OnHover));
			uIEventListener2.onDrop = (UIEventListener.ObjectDelegate)Delegate.Combine(uIEventListener2.onDrop, new UIEventListener.ObjectDelegate(OnDrop));
		}
	}

	private void UnsubscribeEvents()
	{
		if ((bool)Background)
		{
			UIEventListener uIEventListener = UIEventListener.Get(Background);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Remove(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnClick));
			uIEventListener.onRightClick = (UIEventListener.VoidDelegate)Delegate.Remove(uIEventListener.onRightClick, new UIEventListener.VoidDelegate(OnRightClick));
			uIEventListener.onDoubleClick = (UIEventListener.VoidDelegate)Delegate.Remove(uIEventListener.onDoubleClick, new UIEventListener.VoidDelegate(OnDoubleClick));
			uIEventListener.onScroll = (UIEventListener.FloatDelegate)Delegate.Remove(uIEventListener.onScroll, new UIEventListener.FloatDelegate(OnScroll));
			uIEventListener.onHover = (UIEventListener.BoolDelegate)Delegate.Remove(uIEventListener.onHover, new UIEventListener.BoolDelegate(OnHover));
			uIEventListener.onTooltip = (UIEventListener.BoolDelegate)Delegate.Remove(uIEventListener.onTooltip, new UIEventListener.BoolDelegate(OnTooltip));
			uIEventListener.onDrag = (UIEventListener.VectorDelegate)Delegate.Remove(uIEventListener.onDrag, new UIEventListener.VectorDelegate(OnDrag));
			uIEventListener.onDrop = (UIEventListener.ObjectDelegate)Delegate.Remove(uIEventListener.onDrop, new UIEventListener.ObjectDelegate(OnDrop));
		}
		if ((bool)NonDragBackground)
		{
			UIEventListener uIEventListener2 = UIEventListener.Get(NonDragBackground);
			uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Remove(uIEventListener2.onClick, new UIEventListener.VoidDelegate(OnClick));
			uIEventListener2.onRightClick = (UIEventListener.VoidDelegate)Delegate.Remove(uIEventListener2.onRightClick, new UIEventListener.VoidDelegate(OnRightClick));
			uIEventListener2.onDoubleClick = (UIEventListener.VoidDelegate)Delegate.Remove(uIEventListener2.onDoubleClick, new UIEventListener.VoidDelegate(OnDoubleClick));
			uIEventListener2.onScroll = (UIEventListener.FloatDelegate)Delegate.Remove(uIEventListener2.onScroll, new UIEventListener.FloatDelegate(OnScroll));
			uIEventListener2.onHover = (UIEventListener.BoolDelegate)Delegate.Remove(uIEventListener2.onHover, new UIEventListener.BoolDelegate(OnHover));
			uIEventListener2.onDrop = (UIEventListener.ObjectDelegate)Delegate.Remove(uIEventListener2.onDrop, new UIEventListener.ObjectDelegate(OnDrop));
		}
		UnsubscribeItemModsChanged();
	}

	public void TransplantWidgets(UIInventoryGridItem other)
	{
		other.UnsubscribeEvents();
		Icon = other.Icon;
		Background = other.Background;
		NonDragBackground = other.NonDragBackground;
		QuantityLabel = other.QuantityLabel;
		m_BackgroundFadeOut = other.m_BackgroundFadeOut;
		m_HoveredSprite = other.m_HoveredSprite;
		m_SelectedSprite = other.m_SelectedSprite;
		m_MakeLinkSprite = other.m_MakeLinkSprite;
		m_IsLinkedSprite = other.m_IsLinkedSprite;
		m_DragComponent = other.m_DragComponent;
		m_EquipmentSlotSprite = other.m_EquipmentSlotSprite;
		m_SpriteModGlow = other.m_SpriteModGlow;
		WidgetOwner = other.WidgetOwner;
		WidgetOwner.transform.parent = base.transform;
		WidgetOwner.transform.localPosition = Vector3.zero;
		other.WidgetOwner = null;
		other.Icon = null;
		other.Background = null;
		other.NonDragBackground = null;
		other.QuantityLabel = null;
		other.m_BackgroundFadeOut = null;
		other.m_HoveredSprite = null;
		other.m_SelectedSprite = null;
		other.m_MakeLinkSprite = null;
		other.m_IsLinkedSprite = null;
		other.m_DragComponent = null;
		other.m_EquipmentSlotSprite = null;
		other.m_SpriteModGlow = null;
		UIParentDependentBehaviour[] componentsInChildren = WidgetOwner.GetComponentsInChildren<UIParentDependentBehaviour>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].FindParent();
		}
		SubscribeEvents();
		if (InvItem != null)
		{
			SendReload(InvItem.baseItem, InvItem.stackSize);
		}
		RefreshIcon();
	}

	private void Update()
	{
		if (m_Item != null && m_Item.baseItem == null)
		{
			m_Item = null;
			RefreshIcon();
		}
		if (Selected && (m_Item == null || m_Item.baseItem == null))
		{
			Selected = false;
		}
		if (m_SecondTooltipTimer > 0f)
		{
			m_SecondTooltipTimer -= Time.unscaledDeltaTime;
			if (m_SecondTooltipTimer <= 0f)
			{
				ShowSecondTooltip();
			}
		}
		RefreshColor();
	}

	public void AddLinkSprites()
	{
		if (m_MakeLinkSprite == null)
		{
			Transform parent = (WidgetOwner ? WidgetOwner.transform : base.gameObject.transform);
			GameObject gameObject = new GameObject("MakeLink");
			gameObject.transform.parent = parent;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localScale = Icon.transform.localScale;
			gameObject.transform.localRotation = Quaternion.identity;
			gameObject.layer = Background.gameObject.layer;
			m_MakeLinkSprite = gameObject.AddComponent<UISprite>();
			m_MakeLinkSprite.atlas = UIAtlasManager.Instance.Inventory;
			m_MakeLinkSprite.spriteName = "ICO_linkWeapon";
			m_MakeLinkSprite.depth = 2;
			m_MakeLinkSprite.alpha = 0f;
			gameObject = UnityEngine.Object.Instantiate(gameObject);
			gameObject.name = "IsLinked";
			gameObject.transform.parent = parent;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localScale = Icon.transform.localScale;
			gameObject.transform.localRotation = Quaternion.identity;
			gameObject.layer = Background.gameObject.layer;
			m_IsLinkedSprite = gameObject.GetComponent<UISprite>();
			m_IsLinkedSprite.spriteName = "ICO_linkedWeapon";
		}
	}

	public void Block()
	{
		m_Blocked = true;
		RefreshIcon();
	}

	public void Unblock()
	{
		m_Blocked = false;
		RefreshIcon();
	}

	public void Loot(BaseInventory into)
	{
		if ((bool)into && m_Item != null && ItemTakeValid(this, out var _))
		{
			SendQuantity(m_Item.stackSize, into);
		}
	}

	public void Activate()
	{
		if (m_Item == null || Locked)
		{
			return;
		}
		if (m_Item.baseItem.MaxStackSize > 1 && m_Item.stackSize > 1 && !UIGlobalInventory.Instance.DraggingItem && ItemTakeValid(this, out var error))
		{
			UIMessageBox uIMessageBox = ShowSplitDialog(m_Item);
			if (!uIMessageBox)
			{
				return;
			}
			uIMessageBox.OnDialogEnd = delegate(UIMessageBox.Result result, UIMessageBox owner)
			{
				if (result == UIMessageBox.Result.AFFIRMATIVE)
				{
					UIGlobalInventory.Instance.BeginDrag(this, realDrag: false, owner.NumericValue);
				}
			};
		}
		else
		{
			if (Owner.IsStore)
			{
				return;
			}
			if (m_Owner is UIInventoryItemGrid && ((UIInventoryItemGrid)m_Owner).IsExternalContainer)
			{
				Loot(UIInventoryManager.Instance.SelectedCharacter.GetComponent<Inventory>());
			}
			else if (m_Item.baseItem is Consumable)
			{
				if (Owner == UIInventoryManager.Instance.QuickItemsGrid)
				{
					UIInventoryGridItem uIInventoryGridItem = UIInventoryManager.Instance.RowPanel.RowForSelectedPartyMember().ItemGrid.FirstEmptyGridItem();
					if (uIInventoryGridItem != null)
					{
						uIInventoryGridItem.TrySwap(this, splitStack: false);
					}
				}
				else if (UIInventoryManager.Instance.WindowActive() && ItemTransferValid(InvItem, this, UIInventoryManager.Instance.QuickItemsGrid, out error))
				{
					int num = UIInventoryManager.Instance.QuickItemsGrid.Add(m_Item.baseItem, m_Item.stackSize, null);
					if (m_Item.baseItem == null || num <= 0)
					{
						RemoveItem();
					}
					else
					{
						InvItem.SetStackSize(num);
					}
				}
			}
			else
			{
				if (!(m_Item.baseItem is Equippable))
				{
					return;
				}
				if (EquipmentSlot != Equippable.EquipmentSlot.None)
				{
					UIInventoryGridItem uIInventoryGridItem2 = UIInventoryManager.Instance.RowPanel.RowForSelectedPartyMember().ItemGrid.FirstEmptyGridItem();
					if (uIInventoryGridItem2 != null)
					{
						uIInventoryGridItem2.TrySwap(this, splitStack: false);
					}
				}
				else if (UIInventoryManager.Instance.WindowActive())
				{
					Equippable equippable = m_Item.baseItem as Equippable;
					UIInventoryGridItem gridItemForSlot = UIInventoryManager.Instance.Equipment.GetGridItemForSlot(equippable.GetPreferredSlot());
					if (gridItemForSlot != null && gridItemForSlot.gameObject.activeSelf)
					{
						gridItemForSlot.TrySwap(this, splitStack: false);
					}
				}
			}
		}
	}

	public void RefreshColor()
	{
		if (m_HoveredSprite == null)
		{
			return;
		}
		string error = null;
		if (m_Blocked)
		{
			if (InvItem != null)
			{
				m_HoveredSprite.color = UIInventoryManager.Instance.ItemInvalidDropColor;
			}
			else
			{
				m_HoveredSprite.color = UIInventoryManager.Instance.ItemNormalColor;
			}
		}
		else if (m_Hovered)
		{
			if (UIInventoryManager.Instance.DraggingItem)
			{
				if (ItemDropValid(this, out error))
				{
					m_HoveredSprite.color = UIInventoryManager.Instance.ItemValidDropColor;
				}
				else
				{
					m_HoveredSprite.color = UIInventoryManager.Instance.ItemInvalidDropColor;
				}
			}
			else
			{
				m_HoveredSprite.color = UIInventoryManager.Instance.ItemNormalColor;
			}
		}
		else if ((EquipmentSlot != Equippable.EquipmentSlot.None || OrAllowEquipment != Equippable.EquipmentSlot.None || RestrictByFilter != 0) && UIInventoryManager.Instance.DraggingItem && ItemDropValid(this, out error))
		{
			m_HoveredSprite.color = UIInventoryManager.Instance.ItemValidDropColor;
		}
		else if ((bool)m_BackgroundFadeOut && !m_BackgroundFadeOut.enabled && m_HoveredSprite.color != UIInventoryManager.Instance.ItemNormalColor)
		{
			m_BackgroundFadeOut.from = m_HoveredSprite.color;
			m_BackgroundFadeOut.Reset();
			m_BackgroundFadeOut.Play(forward: true);
		}
	}

	private void ShowTooltip()
	{
		if (!TooltipDisabled)
		{
			if (InvItem != null)
			{
				ISelectACharacter selectACharacter = UIWindowManager.FindParentISelectACharacter(base.transform);
				if (selectACharacter == null)
				{
					selectACharacter = UIInventoryManager.Instance;
				}
				CharacterStats selectedCharacter = selectACharacter.SelectedCharacter;
				GameObject owner = null;
				if ((bool)selectedCharacter)
				{
					owner = selectedCharacter.gameObject;
				}
				m_SecondTooltipTimer = GameState.Option.TooltipDelay;
				if ((bool)UIStoreManager.Instance && UIStoreManager.Instance.WindowActive())
				{
					UIAbilityTooltip.GlobalShow(Icon, owner, new Item.StoreItem(InvItem.baseItem, UIStoreManager.Instance.Store, Owner.IsStore));
				}
				else
				{
					UIAbilityTooltip.GlobalShow(Icon, owner, InvItem.baseItem);
				}
			}
			else if (EquipmentSlot != Equippable.EquipmentSlot.None)
			{
				UIActionBarTooltip.GlobalShow(Icon, GUIUtils.GetEquipmentSlotString(EquipmentSlot));
			}
			else
			{
				m_SecondTooltipTimer = 0f;
				UIAbilityTooltip.GlobalHide();
				UIActionBarTooltip.GlobalHide();
			}
		}
		else
		{
			m_SecondTooltipTimer = 0f;
			UIAbilityTooltip.GlobalHide();
			UIActionBarTooltip.GlobalHide();
		}
	}

	private void ShowSecondTooltip()
	{
		if (!TooltipDisabled && InvItem != null)
		{
			ISelectACharacter selectACharacter = UIWindowManager.FindParentISelectACharacter(base.transform);
			if (selectACharacter == null)
			{
				selectACharacter = UIInventoryManager.Instance;
			}
			CharacterStats selectedCharacter = selectACharacter.SelectedCharacter;
			GameObject owner = null;
			if ((bool)selectedCharacter)
			{
				owner = selectedCharacter.gameObject;
			}
			if (!IsEquipped && InvItem != null && InvItem.baseItem is Equippable)
			{
				UIAbilityTooltipManager.Instance.Show(1, UIAbilityTooltipManager.Instance.GetBg(0), owner, UIWidget.Pivot.TopRight, UIInventoryManager.Instance.Equipment.GetComparisonTargets(InvItem.baseItem as Equippable).Cast<ITooltipContent>(), ComparisonOffset);
			}
		}
	}

	private void OnTooltip(GameObject sender, bool over)
	{
		if (over && !m_Tooltipped && !UIGlobalInventory.Instance.DraggingItem)
		{
			m_Tooltipped = true;
			if ((InvItem == null || InvItem.baseItem == null) && (bool)WeaponSetBuddy)
			{
				WeaponSetBuddy.ShowTooltip();
			}
			else
			{
				ShowTooltip();
			}
		}
		else if (!over && m_Tooltipped)
		{
			m_SecondTooltipTimer = 0f;
			m_Tooltipped = false;
			if (!Empty)
			{
				s_TooltipRepeatWindow = Time.realtimeSinceStartup + 0.6f;
				s_TooltipRepeatLast = UICamera.lastTouchPosition;
			}
			UIAbilityTooltip.GlobalHide();
			UIActionBarTooltip.GlobalHide();
		}
	}

	private void OnHover(GameObject go, bool over)
	{
		if ((bool)m_DragComponent)
		{
			m_DragComponent.DragEnabled = !over || Empty;
		}
		if (over)
		{
			if (s_TooltipRepeatWindow + 0.6f > Time.realtimeSinceStartup && (s_TooltipRepeatLast - UICamera.lastTouchPosition).sqrMagnitude < 6400f)
			{
				OnTooltip(go, over);
			}
		}
		else
		{
			OnTooltip(go, over: false);
		}
		m_Hovered = over;
	}

	private void OnDoubleClick(GameObject go)
	{
		UIGlobalInventory.Instance.UnselectAll();
		Activate();
	}

	private void OnDrag(GameObject go, Vector2 disp)
	{
		if (!UIInventoryManager.Instance.DraggingItem && ItemTakeValid(this, out var _) && !GameInput.GetControlkey())
		{
			UIInventoryManager.Instance.BeginDrag(this, realDrag: true);
		}
	}

	private void OnDrop(GameObject go, GameObject other)
	{
		if (UIGlobalInventory.Instance.TryDragDrop())
		{
			TryDropItem(splitStack: false);
		}
	}

	private void OnRightClick(GameObject go)
	{
		if (UIInventoryManager.Instance.DraggingItem)
		{
			TryDropItem(splitStack: true);
		}
		else
		{
			if (InvItem == null)
			{
				return;
			}
			Grimoire component = InvItem.baseItem.GetComponent<Grimoire>();
			if (component != null)
			{
				PartyMemberAI partyMemberAI = (Owner.OwnerGameObject ? Owner.OwnerGameObject.GetComponent<PartyMemberAI>() : null);
				if (!partyMemberAI)
				{
					partyMemberAI = UIAbilityBar.GetSelectedAIForBars();
				}
				if (!partyMemberAI)
				{
					partyMemberAI = GameState.s_playerCharacter.GetComponent<PartyMemberAI>();
				}
				if ((bool)partyMemberAI)
				{
					UIWindowManager.Instance.SuspendFor(UIGrimoireManager.Instance);
					UIGrimoireManager.Instance.SelectCharacter(partyMemberAI);
					UIGrimoireManager.Instance.LoadGrimoire(component, !Owner.IsStore);
					UIGrimoireManager.Instance.ShowWindow();
				}
			}
			else
			{
				Inspect();
			}
		}
	}

	private void Inspect()
	{
		if (Owner.IsStore)
		{
			UIItemInspectManager.ExamineStore(InvItem.BaseItem, Owner.OwnerGameObject);
		}
		else
		{
			UIItemInspectManager.Examine(InvItem.BaseItem, Owner.OwnerGameObject);
		}
	}

	private void OnScroll(GameObject go, float delta)
	{
		if (OnScrolled != null)
		{
			OnScrolled(base.gameObject, delta);
		}
	}

	private void OnClick(GameObject go)
	{
		if (OnClicked != null)
		{
			OnClicked(base.gameObject);
		}
		if (InGameHUD.Instance.CursorMode == InGameHUD.ExclusiveCursorMode.Inspect && InvItem != null)
		{
			Inspect();
		}
		else if (InGameHUD.Instance.CursorMode == InGameHUD.ExclusiveCursorMode.Helwax && InvItem != null)
		{
			Equippable component = InvItem.baseItem.GetComponent<Equippable>();
			if (!component)
			{
				UIGlobalInventory.Instance.PostMessage(GUIUtils.GetText(2334), Icon);
				return;
			}
			if ((bool)component.EquippedOwner)
			{
				UIGlobalInventory.Instance.PostMessage(GUIUtils.GetText(2336), Icon);
				return;
			}
			if ((bool)InvItem.baseItem.GetComponent<EquipmentSoulbind>())
			{
				UIGlobalInventory.Instance.PostMessage(GUIUtils.Format(2335, InvItem.baseItem.Name), Icon);
				return;
			}
			InGameHUD.Instance.EndExclusiveCursor();
			if (!InGameHUD.Instance.HelwaxSource)
			{
				return;
			}
			UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.ACCEPTCANCEL, InGameHUD.Instance.HelwaxSource.Name, GUIUtils.Format(2332, InvItem.baseItem.Name, InGameHUD.Instance.HelwaxSource.Name)).OnDialogEnd = delegate(UIMessageBox.Result result, UIMessageBox sender)
			{
				if (result == UIMessageBox.Result.AFFIRMATIVE)
				{
					Equippable component2 = InvItem.baseItem.GetComponent<Equippable>();
					if ((bool)component2)
					{
						InGameHUD.Instance.TargetHelwax(component2);
					}
				}
			};
		}
		else if (UIInventoryManager.Instance.DraggingItem)
		{
			TryDropItem(splitStack: false);
		}
		else if ((bool)UIInventoryManager.Instance.LinkingActive)
		{
			UIGlobalInventory.Instance.HideMessage();
			if (m_Item != null && LinkAllowed)
			{
				UIInventoryManager.Instance.LinkingActive.CreateLinkTo(this);
			}
			UIInventoryManager.Instance.LinkingActive = null;
		}
		else if (m_IsLinked)
		{
			ClearLink();
			UIGlobalInventory.Instance.HideMessage();
		}
		else if (m_Item == null && LinkAllowed)
		{
			if (!UIInventoryManager.Instance.LinkingActive)
			{
				UIInventoryManager.Instance.LinkingActive = this;
				UIGlobalInventory.Instance.PostMessage("Click another equipped item to link it to this slot.", Icon);
			}
		}
		else
		{
			DoClick();
			RefreshColor();
		}
	}

	private void DoClick()
	{
		if (m_Item == null)
		{
			return;
		}
		string error3;
		if (GameInput.GetShiftkey())
		{
			if (ItemPlayerTakeValid(InvItem, this, out var error))
			{
				StashInventory component = GameState.s_playerCharacter.GetComponent<StashInventory>();
				if ((bool)component && !component.Contains(InvItem))
				{
					InventoryItem item = RemoveItem();
					if (!component.PutItem(item))
					{
						PutItem(item);
					}
				}
			}
			else
			{
				UIGlobalInventory.Instance.PostMessage(error, Icon);
			}
		}
		else if (GameInput.GetControlkey())
		{
			if (Selected)
			{
				UIGlobalInventory.Instance.Unselect(this);
			}
			else
			{
				UIGlobalInventory.Instance.Select(this);
			}
		}
		else if (m_IsLinked)
		{
			UIGlobalInventory.Instance.PostMessage("You can't move a linked item.", Icon);
		}
		else if (ClickSendsTo != null || ClickSendsToPlayer)
		{
			if (ItemTakeValid(this, out var error2) && ItemTransferValid(InvItem, this, ClickSendsTo, out error2))
			{
				int num = 0;
				if (InvItem.baseItem is Currency)
				{
					num = InvItem.stackSize;
				}
				else if (InvItem.baseItem.MaxStackSize > 1 && InvItem.stackSize > 1)
				{
					if (UIStoreManager.Instance.WindowActive())
					{
						UIMessageBox uIMessageBox = ShowSplitDialog(InvItem);
						if ((bool)uIMessageBox)
						{
							uIMessageBox.OnDialogEnd = (UIMessageBox.OnEndDialog)Delegate.Combine(uIMessageBox.OnDialogEnd, (UIMessageBox.OnEndDialog)delegate(UIMessageBox.Result result, UIMessageBox owner)
							{
								if (result == UIMessageBox.Result.AFFIRMATIVE)
								{
									SendQuantity(owner.NumericValue);
								}
							});
						}
					}
					else
					{
						num = InvItem.stackSize;
					}
				}
				else
				{
					num = 1;
				}
				if (num > 0)
				{
					SendQuantity(num);
				}
			}
			else
			{
				UIGlobalInventory.Instance.PostMessage(error2, Icon);
			}
			UICamera component2 = InGameUILayout.NGUICamera.GetComponent<UICamera>();
			if ((bool)component2)
			{
				component2.ReallowTooltip(Background);
				component2.ReallowTooltip(NonDragBackground);
			}
		}
		else if (ItemTakeValid(this, out error3))
		{
			UIInventoryManager.Instance.BeginDrag(this, realDrag: false);
		}
		else
		{
			UIGlobalInventory.Instance.PostMessage(error3, Icon);
		}
	}

	private void SendQuantity(int qty, BaseInventory target = null)
	{
		if (qty == 0)
		{
			return;
		}
		Item item = m_Item.baseItem;
		if (item.MaxStackSize > 1)
		{
			item = item.Prefab;
		}
		int num;
		string error;
		if ((bool)target)
		{
			num = target.AddItem(item, qty);
			if (num > 0 && target.IsPartyMember)
			{
				num = GameState.s_playerCharacter.Inventory.AddItem(m_Item.baseItem, num, -1);
			}
		}
		else if ((bool)ClickSendsTo && ItemTransferValid(m_Item, this, ClickSendsTo, out error))
		{
			num = ClickSendsTo.Add(item, qty, null);
		}
		else
		{
			if (!ClickSendsToPlayer)
			{
				return;
			}
			num = GameState.s_playerCharacter.Inventory.AddItem(item, qty);
		}
		if (num == qty && item is CampingSupplies)
		{
			UIGlobalInventory.Instance.PostMessage(GUIUtils.GetText(2122), Icon);
		}
		if (num >= qty)
		{
			return;
		}
		int num2 = qty - num;
		if (OnItemTake != null)
		{
			OnItemTake(m_Item.baseItem, num2);
		}
		int num3 = m_Item.stackSize - num2;
		if (m_Item.baseItem == null || num3 <= 0)
		{
			if (item.IsPrefab)
			{
				DestroyItem();
			}
			else
			{
				RemoveItem();
			}
		}
		else
		{
			m_Item.SetStackSize(num3);
			RefreshIcon();
		}
		if (m_Owner != null)
		{
			m_Owner.Reload();
		}
	}

	public void CreateLinkTo(UIInventoryGridItem other)
	{
		string error = null;
		if (!ItemTransferValid(other.InvItem, other, this, out error))
		{
			UIGlobalInventory.Instance.PostMessage(error, Icon);
			return;
		}
		m_Item = other.InvItem;
		m_IsLinked = true;
		m_Owner.Put(m_Item, this);
		RefreshIcon();
	}

	public void ClearLink()
	{
		if (m_IsLinked)
		{
			m_IsLinked = false;
			m_Item = null;
			RefreshIcon();
		}
	}

	public bool TrySwap(UIInventoryGridItem other)
	{
		return TrySwap(other, splitStack: false);
	}

	public bool TrySwap(UIInventoryGridItem other, bool splitStack)
	{
		if (other == this)
		{
			return false;
		}
		string error = "";
		if (!ItemSwapValid(other, this, out error))
		{
			UIGlobalInventory.Instance.PostMessage(error, other.Icon);
			return false;
		}
		UIGlobalInventory.Instance.HideMessage();
		if (other.InvItem.stackSize > 1 && m_Item == null && splitStack)
		{
			UIMessageBox uIMessageBox = ShowSplitDialog(other.InvItem);
			if ((bool)uIMessageBox)
			{
				uIMessageBox.OnDialogEnd = (UIMessageBox.OnEndDialog)Delegate.Combine(uIMessageBox.OnDialogEnd, (UIMessageBox.OnEndDialog)delegate(UIMessageBox.Result result, UIMessageBox owner)
				{
					if (result == UIMessageBox.Result.AFFIRMATIVE && TryMergeInto(other.InvItem, owner.NumericValue) == 0)
					{
						UIGlobalInventory.Instance.FinishDrag();
					}
				});
				return true;
			}
			return false;
		}
		int stackSize = other.InvItem.stackSize;
		if (TryMergeInto(other.InvItem, -1) == stackSize)
		{
			Swap(other, int.MaxValue);
			return true;
		}
		other.RemoveItem();
		return false;
	}

	public bool TryPutIn(UIInventoryItemZone to)
	{
		if (ItemTransferValid(InvItem, this, to, out var error))
		{
			InventoryItem item = RemoveItem();
			if (!to.Put(item, null))
			{
				PutItem(item);
				return false;
			}
			return true;
		}
		UIGlobalInventory.Instance.PostMessage(error, Icon);
		return false;
	}

	public bool TryDropItem()
	{
		return TryDropItem(splitStack: false);
	}

	public bool TryDropItem(bool splitStack)
	{
		if (UIGlobalInventory.Instance.MultiDrag)
		{
			if (!Owner.AllowMultiDrop)
			{
				UIGlobalInventory.Instance.CancelDrag();
				return false;
			}
			UIGlobalInventory.Instance.FinishMultidragAt(Owner);
			return true;
		}
		InventoryItem draggedItem = UIInventoryManager.Instance.DraggedItem;
		if (draggedItem == null)
		{
			UIGlobalInventory.Instance.CancelDrag();
			return false;
		}
		string error = "";
		if (!ItemDropValid(this, out error))
		{
			UIGlobalInventory.Instance.CancelDrag();
			UIGlobalInventory.Instance.PostMessage(error, Icon);
			return false;
		}
		UIGlobalInventory.Instance.HideMessage();
		if (draggedItem.stackSize > 1 && m_Item == null && splitStack)
		{
			UIMessageBox uIMessageBox = ShowSplitDialog(draggedItem);
			if ((bool)uIMessageBox)
			{
				uIMessageBox.OnDialogEnd = delegate(UIMessageBox.Result result, UIMessageBox owner)
				{
					if (result == UIMessageBox.Result.AFFIRMATIVE)
					{
						if (TryMergeInto(UIInventoryManager.Instance.DraggedItem, owner.NumericValue) == 0)
						{
							UIInventoryManager.Instance.FinishDrag();
						}
					}
					else
					{
						UIInventoryManager.Instance.CancelDrag();
					}
				};
				return true;
			}
			return false;
		}
		int stackSize = UIInventoryManager.Instance.DraggedItem.stackSize;
		int num = TryMergeInto(UIInventoryManager.Instance.DraggedItem, -1);
		if (num == stackSize)
		{
			InventoryItem draggedItem2 = UIInventoryManager.Instance.DraggedItem;
			if (SwapIsPut || !Owner.CareAboutIndividualSlots)
			{
				if (Owner.Put(draggedItem2, this))
				{
					UIInventoryManager.Instance.FinishDrag();
					return true;
				}
				UIGlobalInventory.Instance.CancelDrag();
				return false;
			}
			if (ItemTransferValid(UIGlobalInventory.Instance.DraggedItem, UIGlobalInventory.Instance.DraggedSource, this, out error, alreadyHeld: true) && Owner.CanPut(draggedItem2, this))
			{
				UIInventoryManager.Instance.FinishDrag();
				UIInventoryManager.Instance.BeginDrag(this, realDrag: false);
				return PutItem(draggedItem2);
			}
			UIGlobalInventory.Instance.CancelDrag();
			return false;
		}
		if (num == 0)
		{
			UIInventoryManager.Instance.FinishDrag();
			return true;
		}
		return false;
	}

	protected int TryMergeInto(InventoryItem item, int upto)
	{
		if (upto < 0 || upto > item.stackSize)
		{
			upto = item.stackSize;
		}
		if (item != null && item.baseItem != null && upto > 0)
		{
			int num = 0;
			if (m_Item != null && m_Item.baseItem != null)
			{
				if (InventoryItem.ItemsCanStack(m_Item.baseItem, item.baseItem) && m_Item.baseItem.MaxStackSize > 1)
				{
					num = upto;
					if (num + m_Item.stackSize > m_Item.baseItem.MaxStackSize)
					{
						num = m_Item.baseItem.MaxStackSize - m_Item.stackSize;
					}
				}
			}
			else
			{
				num = upto;
			}
			if (num > item.baseItem.MaxStackSize && !Owner.InfiniteStacking)
			{
				num = item.baseItem.MaxStackSize;
			}
			if (num < 0)
			{
				num = 0;
			}
			int num2 = item.stackSize - num;
			if (m_Item != null)
			{
				m_Item.SetStackSize(m_Item.stackSize + num);
			}
			else
			{
				if (num == item.stackSize)
				{
					if (PutItem(item))
					{
						RefreshIcon();
						return 0;
					}
					return num;
				}
				num2 += Owner.Add(item.baseItem.Prefab, num, this);
			}
			if (num2 > 0)
			{
				item.SetStackSize(num2);
			}
			else
			{
				GameUtilities.Destroy(item.baseItem.gameObject);
			}
			RefreshIcon();
			return num2;
		}
		return item.stackSize;
	}

	protected UIMessageBox ShowSplitDialog(InventoryItem invitem)
	{
		if (!s_ActiveSplitDialog || !s_ActiveSplitDialog.IsVisible)
		{
			s_ActiveSplitDialog = UIWindowManager.ShowMessageBox(UIMessageBox.ButtonStyle.ACCEPTCANCEL, GUIUtils.GetText(1707), GUIUtils.Format(1708, invitem.baseItem.Name));
			s_ActiveSplitDialog.SetNumericValue(invitem.stackSize, 1, invitem.stackSize);
			UIMessageBox uIMessageBox = s_ActiveSplitDialog;
			uIMessageBox.OnDialogEnd = (UIMessageBox.OnEndDialog)Delegate.Combine(uIMessageBox.OnDialogEnd, (UIMessageBox.OnEndDialog)delegate
			{
				s_ActiveSplitDialog = null;
			});
			return s_ActiveSplitDialog;
		}
		return null;
	}

	protected void Swap(UIInventoryGridItem other, int quantity)
	{
		if (!ItemSwapValid(this, other, out var error))
		{
			UIGlobalInventory.Instance.PostMessage(error, Icon);
			return;
		}
		UIGlobalInventory.Instance.HideMessage();
		UIInventoryManager.Instance.WeaponSets.ClearClonesOf(this);
		UIInventoryManager.Instance.WeaponSets.ClearClonesOf(other);
		if (quantity == 0)
		{
			return;
		}
		if (other.InvItem == null || quantity >= other.InvItem.stackSize)
		{
			InventoryItem item = m_Item;
			InventoryItem item2 = other.m_Item;
			if (!other.Owner.CanPut(item, other) || !Owner.CanPut(item2, this))
			{
				return;
			}
			m_Item = other.m_Item;
			if (other.m_Item != null)
			{
				m_Item = other.m_Owner.Take(item2);
				if (m_Owner.Put(m_Item, this))
				{
					if (other.OnItemTake != null)
					{
						other.OnItemTake(m_Item.baseItem, m_Item.stackSize);
					}
					if (OnItemPut != null)
					{
						OnItemPut(m_Item.baseItem, m_Item.stackSize);
					}
				}
				else
				{
					Debug.LogError("ERROR: inventory swap failed.\n" + Environment.StackTrace);
				}
			}
			other.m_Item = item;
			if (item != null)
			{
				other.m_Item = m_Owner.Take(item);
				if (other.m_Owner.Put(other.m_Item, other))
				{
					if (OnItemTake != null)
					{
						OnItemTake(other.m_Item.baseItem, other.m_Item.stackSize);
					}
					if (other.OnItemPut != null)
					{
						other.OnItemPut(other.m_Item.baseItem, other.m_Item.stackSize);
					}
				}
				else
				{
					Debug.LogError("ERROR: inventory swap failed.\n" + Environment.StackTrace);
				}
			}
			m_Owner.Reload();
			other.m_Owner.Reload();
		}
		else if (m_Item == null)
		{
			Item item3 = other.m_Owner.Remove(other.m_Item.baseItem, quantity);
			m_Owner.Add(item3, quantity, this);
			if (other.OnItemTake != null)
			{
				other.OnItemTake(item3, quantity);
			}
			if (OnItemPut != null)
			{
				OnItemPut(item3, quantity);
			}
			other.RefreshIcon();
			m_Owner.Reload();
		}
		else
		{
			Debug.LogError("Inventory tried to split a stack into an already-occupied cell.");
		}
	}

	public InventoryItem RemoveItem()
	{
		InventoryItem item = m_Item;
		m_Owner.Take(m_Item);
		m_Item = null;
		RefreshIcon();
		return item;
	}

	public void DestroyItem()
	{
		InventoryItem inventoryItem = RemoveItem();
		if (inventoryItem != null && (bool)inventoryItem.baseItem && !inventoryItem.baseItem.IsPrefab)
		{
			Persistence component = inventoryItem.baseItem.gameObject.GetComponent<Persistence>();
			if ((bool)component)
			{
				PersistenceManager.RemoveObject(component);
			}
			GameUtilities.Destroy(inventoryItem.baseItem.gameObject);
		}
	}

	public bool PutItem(InventoryItem item)
	{
		m_Item = item;
		bool result = m_Owner.Put(item, this);
		m_Owner.Reload();
		return result;
	}

	public bool EquipmentModifyValid()
	{
		if ((bool)Owner)
		{
			CharacterStats component = ComponentUtils.GetComponent<CharacterStats>(Owner.OwnerGameObject);
			if (EquipmentSlot != Equippable.EquipmentSlot.None)
			{
				if ((bool)component)
				{
					return !component.IsEquipmentLocked;
				}
				return false;
			}
			return true;
		}
		return true;
	}

	public static bool ItemTakeValid(UIInventoryGridItem from, out string error)
	{
		error = "";
		if (!from)
		{
			return false;
		}
		if (from.InvItem == null)
		{
			return true;
		}
		if (from.Locked)
		{
			return false;
		}
		if (from.Owner.Locked)
		{
			return false;
		}
		if (from.EquipmentSlot != Equippable.EquipmentSlot.None)
		{
			error = GUIUtils.GetText(217);
			if ((bool)from.InvItem.baseItem)
			{
				Equippable component = from.InvItem.baseItem.GetComponent<Equippable>();
				if ((bool)component && !component.CanUnequip())
				{
					error = GUIUtils.Format(2049, component.Name);
					return false;
				}
			}
			if (!from.EquipmentModifyValid())
			{
				return false;
			}
			Equipment selectedEquipment = UIInventoryManager.Instance.Equipment.SelectedEquipment;
			if ((bool)selectedEquipment && selectedEquipment.IsSlotLocked(from.EquipmentSlot))
			{
				return false;
			}
		}
		AccessLevel currentAccessLevel = UIInventoryItemZone.GetCurrentAccessLevel();
		AccessLevel accessLevel = (AccessLevel)Mathf.Min((int)from.Owner.RestrictInternalMove, (int)from.Owner.RestrictExternalTake);
		if (currentAccessLevel < accessLevel)
		{
			switch (accessLevel)
			{
			case AccessLevel.InField:
				error = GUIUtils.GetText(215);
				break;
			case AccessLevel.Rest:
				error = GUIUtils.GetText(216);
				break;
			}
			return false;
		}
		return true;
	}

	public static bool ItemDropValid(UIInventoryGridItem at, out string error)
	{
		return ItemTransferValid(UIInventoryManager.Instance.DraggedItem, UIInventoryManager.Instance.DraggedSource, at, out error, alreadyHeld: true);
	}

	public static bool ItemSwapValid(UIInventoryGridItem from, UIInventoryGridItem to, out string error)
	{
		string error2 = null;
		bool result = ItemTransferValid(from.InvItem, from, to, out error) && ItemTransferValid(to.InvItem, to, from, out error2);
		if (string.IsNullOrEmpty(error))
		{
			error = error2;
		}
		return result;
	}

	public static bool ItemTransferValid(InventoryItem invitem, UIInventoryGridItem from, UIInventoryItemZone to, out string error, bool alreadyHeld = true)
	{
		error = "";
		if (!alreadyHeld && !ItemTakeValid(from, out error))
		{
			return false;
		}
		if (invitem != null && (bool)invitem.baseItem && !invitem.baseItem.CanSell() && to.IsSellBox)
		{
			CanNotSell component = invitem.baseItem.GetComponent<CanNotSell>();
			error = component.MessageBoxMessage.GetText();
			return false;
		}
		if ((bool)from.Owner && (bool)to && (from.Owner.Locked || to.Locked))
		{
			return false;
		}
		if (from.Owner.RestrictTransferTo.Length != 0 && !from.Owner.RestrictTransferTo.Contains(to) && from.Owner != to)
		{
			return false;
		}
		if (((bool)from.Owner && from.Owner.IsStore) != ((bool)to && to.IsStore))
		{
			return false;
		}
		if (from.Owner.ProhibitTransferTo.Contains(to))
		{
			return false;
		}
		if ((bool)to && to.IsQuickslotInventory)
		{
			CharacterStats characterStats = (to.OwnerGameObject ? to.OwnerGameObject.GetComponent<CharacterStats>() : null);
			if ((bool)characterStats && characterStats.HasStatusEffectOfType(StatusEffect.ModifiedStat.CantUseFoodDrinkDrugs))
			{
				Consumable consumable = ((invitem != null && (bool)invitem.baseItem) ? invitem.baseItem.GetComponent<Consumable>() : null);
				if ((bool)consumable && consumable.IsFoodOrDrug)
				{
					error = GUIUtils.Format(2230, CharacterStats.Name(characterStats));
					return false;
				}
			}
		}
		bool num = to != from.Owner;
		AccessLevel currentAccessLevel = UIInventoryItemZone.GetCurrentAccessLevel();
		if (num)
		{
			if ((bool)to && currentAccessLevel < to.RestrictExternalPlace)
			{
				if (to.RestrictExternalPlace == AccessLevel.InField)
				{
					error = GUIUtils.GetText(215);
				}
				else if (to.RestrictExternalPlace == AccessLevel.Rest)
				{
					error = GUIUtils.GetText(216);
				}
				return false;
			}
			if (currentAccessLevel < from.Owner.RestrictExternalTake)
			{
				if (from.Owner.RestrictExternalTake == AccessLevel.InField)
				{
					error = GUIUtils.GetText(215);
				}
				else if (from.Owner.RestrictExternalTake == AccessLevel.Rest)
				{
					error = GUIUtils.GetText(216);
				}
				return false;
			}
		}
		else
		{
			AccessLevel accessLevel = (AccessLevel)Mathf.Min((int)from.Owner.RestrictInternalMove, (int)to.RestrictInternalMove);
			if (currentAccessLevel < accessLevel)
			{
				switch (accessLevel)
				{
				case AccessLevel.InField:
					error = GUIUtils.GetText(215);
					break;
				case AccessLevel.Rest:
					error = GUIUtils.GetText(216);
					break;
				}
				return false;
			}
		}
		return true;
	}

	public static bool ItemTransferValid(InventoryItem invitem, UIInventoryGridItem from, UIInventoryGridItem to, out string error, bool alreadyHeld = false)
	{
		error = "";
		if (invitem == null || invitem.baseItem == null)
		{
			return true;
		}
		if (from == to && UIGlobalInventory.Instance.DragOwnerIsSame())
		{
			return true;
		}
		if (!ItemTransferValid(invitem, from, to ? to.Owner : null, out error, alreadyHeld))
		{
			return false;
		}
		if (!ItemTakeValid(to, out error))
		{
			return false;
		}
		if ((bool)to && (to.Locked || to.Blocked || !to.EquipmentModifyValid()))
		{
			return false;
		}
		if ((bool)to && to.EquipmentSlot != Equippable.EquipmentSlot.None)
		{
			error = GUIUtils.GetText(217);
			Equippable equippable = invitem.baseItem as Equippable;
			EquipmentSoulbind component = invitem.baseItem.GetComponent<EquipmentSoulbind>();
			if (!equippable)
			{
				return false;
			}
			if (!equippable.CanUseSlot(to.EquipmentSlot))
			{
				return false;
			}
			if (to.WeaponSetBuddy != null && !to.WeaponSetBuddy.Empty)
			{
				if (equippable.BothPrimaryAndSecondarySlot)
				{
					error = GUIUtils.GetText(1737);
					return false;
				}
				Equippable equippable2 = to.WeaponSetBuddy.InvItem.baseItem as Equippable;
				if ((bool)equippable2 && equippable2.BothPrimaryAndSecondarySlot)
				{
					error = GUIUtils.GetText(1737);
					return false;
				}
			}
			Equipment selectedEquipment = UIInventoryManager.Instance.Equipment.SelectedEquipment;
			if ((bool)selectedEquipment && selectedEquipment.IsSlotLocked(to.EquipmentSlot))
			{
				return false;
			}
			Equippable.CantEquipReason cantEquipReason = equippable.WhyCantEquip(UIInventoryManager.Instance.SelectedCharacter.gameObject);
			if (cantEquipReason != 0)
			{
				CharacterStats selectedCharacter = UIInventoryManager.Instance.SelectedCharacter;
				switch (cantEquipReason)
				{
				case Equippable.CantEquipReason.ClassMismatch:
					error = GUIUtils.Format(1003, invitem.baseItem.Name, GUIUtils.GetClassString(selectedCharacter.CharacterClass, selectedCharacter.Gender));
					break;
				case Equippable.CantEquipReason.SoulboundToOther:
					error = GUIUtils.Format(2038, equippable.Name, CharacterStats.Name(component.BoundGuid));
					break;
				default:
					error = GUIUtils.Format(1003, invitem.baseItem.Name, CharacterStats.Name(selectedCharacter));
					break;
				}
				return false;
			}
		}
		if (from.EquipmentSlot != Equippable.EquipmentSlot.None)
		{
			Equipment selectedEquipment2 = UIInventoryManager.Instance.Equipment.SelectedEquipment;
			if ((bool)selectedEquipment2 && (bool)to && selectedEquipment2.IsSlotLocked(to.EquipmentSlot))
			{
				return false;
			}
		}
		if ((bool)to && to.EquipmentSlot == Equippable.EquipmentSlot.Grimoire && UIInventoryManager.Instance.SelectedCharacter.EffectDisablesSpellcasting)
		{
			error = GUIUtils.GetText(1738);
			return false;
		}
		if ((bool)to && to.RestrictByFilter != 0 && (invitem.baseItem.FilterType & to.RestrictByFilter) == 0 && (to.OrAllowEquipment == Equippable.EquipmentSlot.None || !(invitem.baseItem is Equippable) || !(invitem.baseItem as Equippable).CanUseSlot(to.OrAllowEquipment)))
		{
			return false;
		}
		return true;
	}

	public static bool ItemPlayerTakeValid(InventoryItem invitem, UIInventoryGridItem from, out string error)
	{
		error = "";
		if (!ItemTakeValid(from, out error))
		{
			return false;
		}
		if (from.Owner.RestrictTransferTo.Length != 0)
		{
			return false;
		}
		if ((bool)from.Owner && from.Owner.IsStore)
		{
			return false;
		}
		if (UIInventoryItemZone.GetCurrentAccessLevel() < from.Owner.RestrictExternalTake)
		{
			if (from.Owner.RestrictExternalTake == AccessLevel.InField)
			{
				error = GUIUtils.GetText(215);
			}
			else if (from.Owner.RestrictExternalTake == AccessLevel.Rest)
			{
				error = GUIUtils.GetText(216);
			}
			return false;
		}
		return true;
	}

	public void SetSlotNumber(int slot)
	{
		m_Slot = slot;
	}

	public void UnsetItem()
	{
		m_Item = null;
		RefreshIcon();
		if (OnItemReload != null)
		{
			OnItemReload(this);
		}
	}

	public void SetItem(InventoryItem item)
	{
		if (item != m_Item)
		{
			m_Item = item;
			RefreshIcon();
			if (OnItemReload != null)
			{
				OnItemReload(this);
			}
		}
	}

	private void RefreshFiltered()
	{
		if ((bool)Icon)
		{
			if (FilterDisabled)
			{
				Icon.alpha = 1f;
			}
			else if (m_Item == null || !UIInventoryFilterManager.Accepts(m_Item.baseItem))
			{
				Icon.alpha = 0.3f;
			}
			else
			{
				Icon.alpha = 1f;
			}
			Icon.alpha *= m_IconAlpha;
		}
	}

	private void SubscribeModsChanged(Equippable newEquipItem)
	{
		if (!(newEquipItem == m_ListenedEquippable))
		{
			if (m_ListenedEquippable != null)
			{
				m_ListenedEquippable.ItemModsChanged -= HandleItemModsChanged;
			}
			m_ListenedEquippable = newEquipItem;
			if (m_ListenedEquippable != null && !m_Item.baseItem.Unique)
			{
				m_ListenedEquippable.ItemModsChanged += HandleItemModsChanged;
			}
		}
	}

	private void UnsubscribeItemModsChanged()
	{
		if (m_ListenedEquippable != null)
		{
			DetermineAndSetEquipModGlowColor(m_ListenedEquippable);
			m_ListenedEquippable.ItemModsChanged -= HandleItemModsChanged;
		}
		m_ListenedEquippable = null;
	}

	private void HandleItemModsChanged(Equippable equipItem)
	{
		if (!(equipItem != m_ListenedEquippable))
		{
			DetermineAndSetEquipModGlowColor(m_ListenedEquippable);
		}
	}

	public static void SetRefreshBlock(bool block)
	{
		s_BlockRefresh = block;
	}

	public void RefreshIcon()
	{
		if (s_BlockRefresh || !Icon)
		{
			return;
		}
		Init();
		bool flag = false;
		m_IconAlpha = 1f;
		Texture mainTexture = Icon.mainTexture;
		if (m_Item != null && m_Item.baseItem != null)
		{
			flag = true;
			Icon.mainTexture = m_Item.baseItem.GetIconTexture();
			Icon.MakePixelPerfect();
			Equippable equippable = m_Item.baseItem as Equippable;
			if (m_Item.baseItem.Unique)
			{
				UnsubscribeItemModsChanged();
			}
			else if (equippable != null)
			{
				if (equippable != m_ListenedEquippable)
				{
					SubscribeModsChanged(equippable);
				}
			}
			else
			{
				UnsubscribeItemModsChanged();
			}
			DetermineAndSetEquipModGlowColor(equippable);
			if (LinkAllowed)
			{
				m_MakeLinkSprite.alpha = 0f;
				if (m_IsLinked)
				{
					m_IsLinkedSprite.alpha = 1f;
				}
				else
				{
					m_IsLinkedSprite.alpha = 0f;
				}
			}
			if (QuantityLabel != null)
			{
				if (m_Item.stackSize > 1)
				{
					QuantityLabel.text = m_Item.stackSize.ToString();
					QuantityLabel.gameObject.SetActive(value: true);
				}
				else
				{
					QuantityLabel.gameObject.SetActive(value: false);
				}
			}
		}
		else if ((bool)WeaponSetBuddy && WeaponSetBuddy.InvItem != null && (bool)WeaponSetBuddy.InvItem.baseItem)
		{
			Equippable component = WeaponSetBuddy.InvItem.baseItem.GetComponent<Equippable>();
			if (component.BothPrimaryAndSecondarySlot)
			{
				flag = true;
				Icon.mainTexture = component.GetIconTexture();
				Icon.MakePixelPerfect();
				m_IconAlpha = 0.35f;
				DetermineAndSetEquipModGlowColor(WeaponSetBuddy.InvItem.baseItem as Equippable);
			}
		}
		if (!flag)
		{
			Icon.mainTexture = null;
			m_IconAlpha = 0f;
			UnsubscribeItemModsChanged();
			SetModGlow(GlowType.NONE);
			if ((bool)QuantityLabel)
			{
				QuantityLabel.gameObject.SetActive(value: false);
			}
		}
		bool flag2 = false;
		UIWidget component2 = Background.GetComponent<UIWidget>();
		if ((bool)component2)
		{
			component2.alpha = 1f;
		}
		if (m_Blocked)
		{
			if (HideWhenBlocked)
			{
				if ((bool)component2)
				{
					component2.alpha = 0f;
				}
			}
			else
			{
				flag2 = true;
			}
		}
		else if (LinkAllowed)
		{
			m_MakeLinkSprite.alpha = 1f;
			m_IsLinkedSprite.alpha = 0f;
		}
		UISprite uISprite = component2 as UISprite;
		if ((bool)uISprite)
		{
			if (uISprite.spriteName != "inv_lockedSlot")
			{
				m_OriginalBgSpriteName = uISprite.spriteName;
			}
			if (flag2)
			{
				uISprite.atlas = UIAtlasManager.Instance.InventoryBack;
				if (EquipmentSlot != Equippable.EquipmentSlot.None)
				{
					uISprite.transform.localPosition = new Vector3(uISprite.transform.localPosition.x, uISprite.transform.localPosition.y, 0f);
				}
			}
			else
			{
				uISprite.atlas = UIAtlasManager.Instance.Inventory;
				if (EquipmentSlot != Equippable.EquipmentSlot.None)
				{
					uISprite.transform.localPosition = new Vector3(uISprite.transform.localPosition.x, uISprite.transform.localPosition.y, -2.5f);
				}
			}
			uISprite.spriteName = (flag2 ? "inv_lockedSlot" : m_OriginalBgSpriteName);
		}
		RefreshFiltered();
		if (Icon.mainTexture != mainTexture && (bool)Icon.panel)
		{
			Icon.panel.Refresh();
		}
		if ((bool)m_EquipmentSlotSprite)
		{
			m_EquipmentSlotSprite.alpha = (((!Icon || !Icon.enabled || !Icon.gameObject.activeSelf || !(Icon.alpha > 0f)) && !Blocked) ? 1f : 0f);
		}
	}
}
