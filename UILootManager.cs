using System;
using UnityEngine;

public class UILootManager : UIHudWindow, ISelectACharacter
{
	public UILabel Title;

	private bool m_CurrentStash;

	private Container m_Container;

	private bool m_WantsToHide;

	private bool m_WantsCheckHide;

	private Inventory m_PendingToStash;

	public UIMultiSpriteImageButton TakeAllButton;

	public UILootPartySelector Selector;

	public UIInventoryItemGrid ItemGrid;

	public UIInventoryItemGrid PartyGrid;

	public UIAnchorToWorld WorldAnchor;

	public GameObject CloseButton;

	public GameObject PortraitParent;

	public UILootStashIconManager StashIcons;

	private PartyMemberAI m_SelectLooter;

	public static UILootManager Instance { get; private set; }

	public bool SelectedStash => m_CurrentStash;

	public CharacterStats SelectedCharacter { get; private set; }

	public event SelectedCharacterChanged OnSelectedCharacterChanged;

	private void Awake()
	{
		Instance = this;
		m_PendingToStash = base.gameObject.AddComponent<Inventory>();
		m_PendingToStash.OverrideRedirectToPlayer = true;
		m_PendingToStash.MaxItems = 16;
		PartyMemberAI.OnAnySelectionChanged += OnSelectionChanged;
		UIInventoryItemGrid itemGrid = ItemGrid;
		itemGrid.OnItemTake = (UIInventoryItemZone.ItemSwapDelegate)Delegate.Combine(itemGrid.OnItemTake, new UIInventoryItemZone.ItemSwapDelegate(OnItemLooted));
	}

	protected override void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		if ((bool)UIGlobalInventory.Instance)
		{
			UIGlobalInventory.Instance.OnDraggingChanged -= OnDraggingChanged;
		}
		UnsubscribeStash();
		PartyMemberAI.OnAnySelectionChanged -= OnSelectionChanged;
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
	}

	private void Start()
	{
		UIMultiSpriteImageButton takeAllButton = TakeAllButton;
		takeAllButton.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(takeAllButton.onClick, new UIEventListener.VoidDelegate(OnTakeAll));
		UIInventoryItemGrid partyGrid = PartyGrid;
		partyGrid.OnItemPut = (UIInventoryItemZone.ItemSwapDelegate)Delegate.Combine(partyGrid.OnItemPut, new UIInventoryItemZone.ItemSwapDelegate(OnInventoryChanged));
		UIInventoryItemGrid partyGrid2 = PartyGrid;
		partyGrid2.OnItemTake = (UIInventoryItemZone.ItemSwapDelegate)Delegate.Combine(partyGrid2.OnItemTake, new UIInventoryItemZone.ItemSwapDelegate(OnInventoryChanged));
		Selector.OnSelectCharacter += OnSelectCharacter;
		Selector.OnSelectStash += OnSelectStash;
		UIEventListener uIEventListener = UIEventListener.Get(Selector.StashButton);
		uIEventListener.onDoubleClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onDoubleClick, new UIEventListener.VoidDelegate(OnDoubleClickStash));
		UIGlobalInventory.Instance.OnDraggingChanged += OnDraggingChanged;
	}

	private void OnSelectStash(GameObject sender)
	{
		SelectStash();
	}

	private void OnSelectCharacter(GameObject sender)
	{
		SelectPartyMember(sender.GetComponentInParent<UILootPartyIcon>().PartyMember);
	}

	private void Update()
	{
		if (!WindowActive())
		{
			return;
		}
		if ((bool)m_SelectLooter)
		{
			SelectPartyMember(m_SelectLooter);
			m_SelectLooter = null;
		}
		if (GameInput.GetControlUp(MappedControl.TAKE_ALL))
		{
			OnTakeAll(null);
		}
		if (m_WantsCheckHide)
		{
			m_WantsCheckHide = false;
			if (!UIGlobalInventory.Instance.DraggingItem)
			{
				HideWindow();
			}
			else
			{
				m_WantsToHide = true;
			}
		}
		if ((bool)WorldAnchor)
		{
			WorldAnchor.UpdatePosition();
		}
	}

	private void OnSelectionChanged(object sender, EventArgs e)
	{
		if (WindowActive())
		{
			GameObject selectedForBars = UIAbilityBar.GetSelectedForBars();
			if ((bool)selectedForBars)
			{
				SelectPartyMember(selectedForBars.GetComponent<PartyMemberAI>());
			}
		}
	}

	private void OnItemStashed(BaseInventory sender, Item item, int qty)
	{
		StashIcons.Looted(item);
	}

	private void OnItemLooted(Item baseItem, int quantity)
	{
		if (ItemGrid.Inventory.Empty())
		{
			m_WantsCheckHide = true;
		}
	}

	private void OnInventoryChanged(Item item, int quantity)
	{
		Selector.ReloadParty();
		if (ItemGrid.Inventory.Empty() && m_WantsToHide)
		{
			m_WantsCheckHide = true;
		}
	}

	private void OnDraggingChanged(bool dragging)
	{
		if (!(ItemGrid.Inventory == null) && ItemGrid.Inventory.Empty() && m_WantsToHide)
		{
			m_WantsCheckHide = true;
		}
	}

	private void OnDoubleClickStash(GameObject sender)
	{
		TakeAll(GameState.s_playerCharacter.Inventory.StashInventory);
	}

	private void OnTakeAll(GameObject go)
	{
		if (m_CurrentStash)
		{
			TakeAll(GameState.s_playerCharacter.Inventory.StashInventory);
		}
		else
		{
			TakeAll(SelectedCharacter.GetComponent<Inventory>());
		}
	}

	public void TakeAll(BaseInventory target)
	{
		GlobalAudioPlayer.SPlay(UIAudioList.UIAudioType.TakeAll);
		ItemGrid.LootAll(target);
		if (ItemGrid.Inventory.Empty())
		{
			HideWindow();
		}
	}

	public void SelectStash()
	{
		m_CurrentStash = true;
		PortraitParent.gameObject.SetActive(value: false);
		PartyGrid.LoadInventory(m_PendingToStash);
		Selector.UpdateSelection();
	}

	public void SelectPartyMember(PartyMemberAI pai)
	{
		if ((bool)pai)
		{
			if (pai.Secondary)
			{
				pai = ((!pai.Summoner) ? GameState.s_playerCharacter.GetComponent<PartyMemberAI>() : pai.Summoner.GetComponent<PartyMemberAI>());
			}
			SelectedCharacter = (pai ? pai.GetComponent<CharacterStats>() : null);
			m_CurrentStash = false;
			PortraitParent.gameObject.SetActive(value: true);
			PartyGrid.LoadInventory(pai.Inventory);
			PartyGrid.gameObject.SetActive(value: true);
			if (this.OnSelectedCharacterChanged != null)
			{
				this.OnSelectedCharacterChanged(SelectedCharacter);
			}
		}
		else
		{
			Selector.UpdateSelection();
			PartyGrid.gameObject.SetActive(value: false);
		}
	}

	public void SetData(PartyMemberAI actor, Inventory loot, GameObject looting)
	{
		m_Container = looting.GetComponent<Container>();
		CombinedInventory combinedInventory = loot as CombinedInventory;
		if ((bool)combinedInventory)
		{
			Title.text = combinedInventory.GroupName;
		}
		else if ((bool)m_Container)
		{
			Title.text = ((!string.IsNullOrEmpty(m_Container.ManualLabelName)) ? m_Container.ManualLabelName : m_Container.LabelName.GetText());
			if (m_Container.StealingFactionID != 0)
			{
				Title.text += GUIUtils.Format(1731, GUIUtils.GetText(1979));
			}
		}
		else
		{
			Title.text = GUIUtils.GetText(407);
		}
		loot.Sort(BaseInventory.CompareItemsForShop);
		if ((bool)WorldAnchor)
		{
			WorldAnchor.SetAnchor(looting);
		}
		Selector.LoadParty();
		if ((bool)actor && !m_CurrentStash && !actor.Inventory.IsFull)
		{
			m_SelectLooter = actor;
		}
		else
		{
			SelectStash();
		}
		ItemGrid.LoadInventory(loot);
	}

	private void SubscribeStash()
	{
		if ((bool)GameState.s_playerCharacter)
		{
			QuestInventory component = GameState.s_playerCharacter.GetComponent<QuestInventory>();
			if ((bool)component)
			{
				component.OnAdded += OnItemStashed;
			}
			CraftingInventory component2 = GameState.s_playerCharacter.GetComponent<CraftingInventory>();
			if ((bool)component2)
			{
				component2.OnAdded += OnItemStashed;
			}
		}
	}

	private void UnsubscribeStash()
	{
		if ((bool)GameState.s_playerCharacter)
		{
			QuestInventory component = GameState.s_playerCharacter.GetComponent<QuestInventory>();
			if ((bool)component)
			{
				component.OnAdded -= OnItemStashed;
			}
			CraftingInventory component2 = GameState.s_playerCharacter.GetComponent<CraftingInventory>();
			if ((bool)component2)
			{
				component2.OnAdded -= OnItemStashed;
			}
		}
	}

	protected override bool Hide(bool forced)
	{
		if (UIGlobalInventory.Instance.DraggingItem)
		{
			if (!forced)
			{
				return false;
			}
			UIGlobalInventory.Instance.CancelDrag();
		}
		ItemGrid.Inventory.CloseInventory();
		m_WantsToHide = false;
		m_WantsCheckHide = false;
		UIGlobalInventory.Instance.HideMessage();
		UnsubscribeStash();
		StashIcons.Clear();
		foreach (InventoryItem item in m_PendingToStash)
		{
			GameState.s_playerCharacter.GetComponent<StashInventory>().PutItem(item);
		}
		m_PendingToStash.ClearInventory(deleteItems: false);
		return base.Hide(forced);
	}

	protected override void Show()
	{
		Selector.LoadParty();
		m_WantsToHide = false;
		UIGlobalInventory.Instance.HideMessage();
		SubscribeStash();
		SetPosition(InGameUILayout.ScreenToNgui(GameInput.MousePosition) + new Vector3(187f, -59f, 0f));
	}
}
