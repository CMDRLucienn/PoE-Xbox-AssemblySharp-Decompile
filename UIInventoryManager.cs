using System;
using System.Linq;
using UnityEngine;

public class UIInventoryManager : UIHudWindow, ISelectACharacterMutable, ISelectACharacter
{
	public Color ItemHoveredColor;

	public Color ItemValidDropColor;

	public Color ItemInvalidDropColor;

	public Color ItemNormalColor;

	public float HoveredAlpha = 0.2f;

	public UITexture PaperdollRegion;

	public UIInventoryItemGrid QuickItemsGrid;

	public bool StashAccess;

	[NonSerialized]
	[HideInInspector]
	public UIInventoryGridItem LinkingActive;

	private PartyMemberAI m_SelectedPartyMember;

	private UIInventoryRowPanel m_RowPanel;

	private UIInventoryEquipment m_Equipment;

	private UIInventoryWeaponSetManager m_WeaponSets;

	private UIPanel m_Panel;

	private int m_framesTillShow;

	public Texture2D DefaultItem;

	public Texture2D DefaultItemLarge;

	private int m_ReloadPaperdoll;

	private float m_PaperdollAngle;

	private bool m_IsReloading;

	private bool m_DoReload;

	private bool m_DoShowStash;

	public static UIInventoryManager Instance { get; private set; }

	public InventoryItem DraggedItem => UIGlobalInventory.Instance.DraggedItem;

	public bool DraggingItem => UIGlobalInventory.Instance.DraggingItem;

	public UIInventoryGridItem DraggedSource => UIGlobalInventory.Instance.DraggedSource;

	public CharacterStats SelectedCharacter
	{
		get
		{
			return UIGlobalSelectAPartyMember.Instance.SelectedCharacter;
		}
		set
		{
			UIGlobalSelectAPartyMember.Instance.SelectedCharacter = value;
		}
	}

	public UIInventoryEquipment Equipment => m_Equipment;

	public UIInventoryRowPanel RowPanel => m_RowPanel;

	public UIInventoryWeaponSetManager WeaponSets => m_WeaponSets;

	public override int CyclePosition => 0;

	public event SelectedCharacterChanged OnSelectedCharacterChanged;

	public void BeginDrag(UIInventoryGridItem item, bool realDrag)
	{
		UIGlobalInventory.Instance.BeginDrag(item, realDrag);
	}

	public void FinishDrag()
	{
		UIGlobalInventory.Instance.FinishDrag();
	}

	public void CancelDrag()
	{
		UIGlobalInventory.Instance.CancelDrag();
	}

	private void Awake()
	{
		Instance = this;
		UIGlobalSelectAPartyMember.Instance.OnSelectedCharacterChanged += OnGlobalSelectionChanged;
		PaperdollRegion.color = new Color(0f, 0f, 0f, 1f);
		ItemHoveredColor.a = Instance.HoveredAlpha;
		ItemValidDropColor.a = Instance.HoveredAlpha;
		ItemInvalidDropColor.a = Instance.HoveredAlpha;
		m_Panel = GetComponent<UIPanel>();
	}

	private void Start()
	{
		m_RowPanel = GetComponentsInChildren<UIInventoryRowPanel>(includeInactive: true)[0];
		m_Equipment = GetComponentsInChildren<UIInventoryEquipment>(includeInactive: true)[0];
		m_WeaponSets = GetComponentsInChildren<UIInventoryWeaponSetManager>(includeInactive: true)[0];
		UIEventListener uIEventListener = UIEventListener.Get(PaperdollRegion.gameObject);
		uIEventListener.onDrag = (UIEventListener.VectorDelegate)Delegate.Combine(uIEventListener.onDrag, new UIEventListener.VectorDelegate(OnDragPaperdoll));
		UIEventListener uIEventListener2 = UIEventListener.Get(PaperdollRegion.gameObject);
		uIEventListener2.onDrop = (UIEventListener.ObjectDelegate)Delegate.Combine(uIEventListener2.onDrop, new UIEventListener.ObjectDelegate(OnDropPaperdoll));
		UIEventListener uIEventListener3 = UIEventListener.Get(PaperdollRegion.gameObject);
		uIEventListener3.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener3.onClick, new UIEventListener.VoidDelegate(OnClickPaperdoll));
		Hide(forced: false);
	}

	protected override void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
	}

	public bool TryEquipDragged()
	{
		Equippable equippable = DraggedItem.baseItem as Equippable;
		if ((bool)equippable)
		{
			UIInventoryGridItem gridItemForSlot = Equipment.GetGridItemForSlot(equippable.GetPreferredSlot());
			if (gridItemForSlot != null && gridItemForSlot.gameObject.activeSelf)
			{
				return gridItemForSlot.TryDropItem();
			}
		}
		return false;
	}

	public bool TryEquipFrom(UIInventoryGridItem grid)
	{
		Equippable equippable = grid.InvItem.baseItem as Equippable;
		if ((bool)equippable)
		{
			UIInventoryGridItem gridItemForSlot = Equipment.GetGridItemForSlot(equippable.GetPreferredSlot());
			if (gridItemForSlot != null)
			{
				return gridItemForSlot.TrySwap(grid);
			}
		}
		return false;
	}

	private void OnDragPaperdoll(GameObject sender, Vector2 disp)
	{
		m_PaperdollAngle += disp.x * 0.5f;
	}

	private void OnClickPaperdoll(GameObject sender)
	{
		OnDropPaperdoll(sender, null);
	}

	private void OnDropPaperdoll(GameObject sender, GameObject other)
	{
		if (!DraggingItem)
		{
			return;
		}
		Consumable consumable = DraggedItem.baseItem as Consumable;
		if (!UIInventoryGridItem.ItemTransferValid(UIGlobalInventory.Instance.DraggedItem, UIGlobalInventory.Instance.DraggedSource, (UIInventoryItemZone)null, out string error, alreadyHeld: true))
		{
			CancelDrag();
			UIGlobalInventory.Instance.PostMessage(error, PaperdollRegion);
		}
		else
		{
			if (TryEquipDragged())
			{
				return;
			}
			if (!GameState.InCombat && (bool)consumable && !consumable.GetComponent<AttackBase>())
			{
				if (consumable.CanUse(SelectedCharacter))
				{
					Item baseItem = DraggedItem.baseItem;
					((Consumable)baseItem).UseImmediately(SelectedCharacter.gameObject);
					GlobalAudioPlayer.Instance.Play(baseItem, GlobalAudioPlayer.UIInventoryAction.UseItem);
				}
				else
				{
					UIGlobalInventory.Instance.PostMessage(consumable.WhyCannotUse(SelectedCharacter), PaperdollRegion);
				}
			}
			else
			{
				CancelDrag();
				if (!GameState.InCombat && (bool)consumable)
				{
					UIGlobalInventory.Instance.PostMessage(consumable.WhyCannotUse(SelectedCharacter), PaperdollRegion);
				}
			}
		}
	}

	private void OnStash(GameObject go)
	{
		if (!DraggingItem)
		{
			UIStashManager.Instance.ShowWindow();
		}
	}

	public void RefreshPartyMemberAppearance()
	{
		if (!SelectedCharacter || !SelectedCharacter.IsEquipmentLocked)
		{
			SelectedCharacter.GetComponent<NPCAppearance>().Generate();
		}
		ReloadPaperdoll();
	}

	private void OnGlobalSelectionChanged(CharacterStats stats)
	{
		if (WindowActive())
		{
			LoadCharacter(stats);
		}
		else
		{
			Equipment.UpdateSelectedCharacter(stats);
		}
	}

	public void SelectPartyMember(int index)
	{
		if (index < 0)
		{
			return;
		}
		foreach (PartyMemberAI onlyPrimaryPartyMember in PartyMemberAI.OnlyPrimaryPartyMembers)
		{
			if (index == 0)
			{
				SelectPartyMember(onlyPrimaryPartyMember);
				GlobalAudioPlayer.Instance.Play(UIAudioList.UIAudioType.ButtonUp);
				break;
			}
			index--;
		}
	}

	public void SelectPartyMember(MonoBehaviour partyMember)
	{
		if ((bool)partyMember)
		{
			SelectedCharacter = partyMember.GetComponent<CharacterStats>();
		}
	}

	public void LoadCharacter(CharacterStats character)
	{
		if ((bool)character)
		{
			m_PaperdollAngle = 0f;
			m_SelectedPartyMember = character.GetComponent<PartyMemberAI>();
			if ((bool)m_SelectedPartyMember && (bool)m_SelectedPartyMember.QuickbarInventory)
			{
				QuickItemsGrid.LoadInventory(m_SelectedPartyMember.QuickbarInventory);
			}
			if (this.OnSelectedCharacterChanged != null)
			{
				this.OnSelectedCharacterChanged(SelectedCharacter);
			}
			if (WindowActive())
			{
				ReloadPaperdoll();
			}
		}
	}

	public void ReloadStats()
	{
		if (!m_IsReloading)
		{
			m_DoReload = true;
			m_IsReloading = true;
			if (this.OnSelectedCharacterChanged != null)
			{
				this.OnSelectedCharacterChanged(SelectedCharacter);
			}
			m_IsReloading = false;
		}
	}

	public void ReloadTints()
	{
		PE_Paperdoll.ReloadTints();
	}

	public void ReloadPaperdoll()
	{
		m_ReloadPaperdoll = 1;
	}

	public override void HandleInput()
	{
		SelectPartyMember(GameInput.NumberPressed - 1);
		if (GameInput.GetControlDownWithRepeat(MappedControl.NEXT_TAB, handle: true))
		{
			if (GameInput.GetShiftkey())
			{
				SelectPartyMember(PartyHelper.SeekPreviousPartyMember(m_SelectedPartyMember));
			}
			else
			{
				SelectPartyMember(PartyHelper.SeekNextPartyMember(m_SelectedPartyMember));
			}
			GlobalAudioPlayer.Instance.Play(UIAudioList.UIAudioType.ButtonUp);
		}
		if (GameInput.GetKeyUp(KeyCode.LeftArrow) || GameInput.GetKeyUp(KeyCode.UpArrow))
		{
			SelectPartyMember(PartyHelper.SeekPreviousPartyMember(m_SelectedPartyMember));
			GlobalAudioPlayer.Instance.Play(UIAudioList.UIAudioType.ButtonUp);
		}
		if (GameInput.GetKeyUp(KeyCode.RightArrow) || GameInput.GetKeyDown(KeyCode.DownArrow))
		{
			SelectPartyMember(PartyHelper.SeekNextPartyMember(m_SelectedPartyMember));
			GlobalAudioPlayer.Instance.Play(UIAudioList.UIAudioType.ButtonUp);
		}
		base.HandleInput();
	}

	private void Update()
	{
		if (!WindowActive())
		{
			return;
		}
		if (m_ReloadPaperdoll == 2)
		{
			m_ReloadPaperdoll = 0;
			try
			{
				PE_Paperdoll.LoadCharacter(m_SelectedPartyMember.gameObject);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception, this);
			}
		}
		if (m_DoShowStash)
		{
			m_DoShowStash = false;
			UIStashManager.Instance.ShowWindow();
		}
		if (m_DoReload)
		{
			m_DoReload = false;
			m_IsReloading = true;
			if (this.OnSelectedCharacterChanged != null)
			{
				this.OnSelectedCharacterChanged(SelectedCharacter);
			}
			m_IsReloading = false;
		}
		if (!(SelectedCharacter != null))
		{
			return;
		}
		if (Instance.DraggingItem)
		{
			Consumable consumable = DraggedItem.baseItem as Consumable;
			if ((bool)consumable && consumable.CanUseViaPaperdoll && !GameState.InCombat)
			{
				PaperdollRegion.color = new Color(1f, 1f, 1f, 5f / 51f);
			}
		}
		else
		{
			PaperdollRegion.color = new Color(0f, 0f, 0f, 1f);
		}
		PE_Paperdoll.LookAt(m_PaperdollAngle);
	}

	private void LateUpdate()
	{
		if (m_ReloadPaperdoll == 1)
		{
			m_ReloadPaperdoll = 2;
		}
		if (base.IsVisible && m_Panel.alpha == 0f)
		{
			if (m_framesTillShow > 0)
			{
				m_framesTillShow--;
				return;
			}
			m_Panel.alpha = 1f;
			m_Panel.SetAlphaRecursive(1f, rebuildList: false);
		}
	}

	public void ImmediateStashAccess()
	{
		ShowWindow();
		m_DoShowStash = true;
		StashAccess = true;
	}

	protected override void Unsuspended()
	{
		LoadCharacter(SelectedCharacter);
		ReloadStats();
		base.Unsuspended();
	}

	protected override void Show()
	{
		m_Equipment.Init();
		m_WeaponSets.Init();
		try
		{
			PE_Paperdoll.CreateCameraInventory();
			PE_Paperdoll.SetRenderSize(new Rect(0f, 0f, PaperdollRegion.transform.localScale.x, PaperdollRegion.transform.localScale.y));
		}
		catch (Exception exception)
		{
			Debug.LogException(exception, this);
		}
		PaperdollRegion.mainTexture = PE_Paperdoll.RenderImage;
		QuickItemsGrid.Reload();
		m_RowPanel.ReloadParty();
		UIGlobalInventory.Instance.HideMessage();
		LoadCharacter(SelectedCharacter);
		m_Panel.alpha = 0f;
		m_Panel.SetAlphaRecursive(0f, rebuildList: false);
		m_framesTillShow = 1;
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
		StashAccess = false;
		PE_Paperdoll.DisableCamera();
		UIGlobalInventory.Instance.HideMessage();
		UIInventoryFilterManager.ClearFilters();
		UIItemsPopupManager.Instance.HideWindow();
		UIStashManager.Instance.HideWindow();
		PartyMemberAI[] partyMembers = PartyMemberAI.PartyMembers;
		foreach (PartyMemberAI partyMemberAI in partyMembers)
		{
			if (!(partyMemberAI != null))
			{
				continue;
			}
			CharacterStats component = partyMemberAI.GetComponent<CharacterStats>();
			if ((bool)component && component.IsEquipmentLocked)
			{
				continue;
			}
			Equipment component2 = partyMemberAI.GetComponent<Equipment>();
			if (!(component2 != null) || component2.CurrentItems == null)
			{
				continue;
			}
			AnimationController component3 = partyMemberAI.GetComponent<AnimationController>();
			if (component3 != null)
			{
				Equippable primaryWeapon = component2.CurrentItems.PrimaryWeapon;
				if (primaryWeapon != null && primaryWeapon is Weapon)
				{
					component3.Stance = (int)(primaryWeapon as Weapon).AnimationStance;
				}
				else
				{
					component3.Stance = 0;
				}
			}
			if (component2.CurrentItems.AlternateWeaponSets.Where((WeaponSet ws) => !ws.Empty()).AnyX(2))
			{
				TutorialManager.STriggerTutorialsOfTypeFast(TutorialManager.ExclusiveTriggerType.INVENTORY_CLOSED_WITH_WEAPON_SET);
			}
		}
		return base.Hide(forced);
	}
}
