using System;
using UnityEngine;

public class UIInventoryWeaponSet : UIInventoryItemZone
{
	public UIMultiSpriteImageButton Button;

	public UILabel NumeralLabel;

	public UIInventoryGridItem PrimarySlot;

	public UIInventoryGridItem SecondarySlot;

	private int m_Set;

	private Equipment m_SelectedEquipment;

	private bool m_Disabled;

	public WeaponSet BackingSet { get; private set; }

	public int BackingSetIndex { get; private set; }

	public bool Disabled
	{
		get
		{
			return m_Disabled;
		}
		set
		{
			m_Disabled = value;
			if (m_Disabled)
			{
				PrimarySlot.Block();
				SecondarySlot.Block();
				Button.enabled = false;
				Button.Label.color = Color.grey;
			}
			else
			{
				PrimarySlot.Unblock();
				SecondarySlot.Unblock();
				Button.enabled = true;
				Button.Label.color = Color.white;
			}
		}
	}

	private void Start()
	{
		UIMultiSpriteImageButton button = Button;
		button.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(button.onClick, new UIEventListener.VoidDelegate(OnClick));
	}

	public void Load(GameObject selected)
	{
		base.OwnerGameObject = selected;
		m_SelectedEquipment = selected.GetComponent<Equipment>();
		Reload();
	}

	protected override InventoryItem DoTake(InventoryItem item)
	{
		if (!(item.baseItem is Equippable))
		{
			Debug.LogError("Tried to remove an item that wasn't equippable.");
		}
		else
		{
			TryPlayTakeSound(item);
			m_SelectedEquipment.UnEquipWeapon((Equippable)item.baseItem, m_Set);
			UIInventoryManager.Instance.RefreshPartyMemberAppearance();
			UIInventoryManager.Instance.ReloadStats();
		}
		return item;
	}

	protected override bool DoCanPut(InventoryItem item, UIInventoryGridItem where)
	{
		return item.baseItem is Equippable;
	}

	protected override bool DoPut(InventoryItem item, UIInventoryGridItem where)
	{
		if (!CanPut(item, where))
		{
			Debug.LogError("Tried to equip an item that wasn't equippable.");
			return false;
		}
		TryPlayPutSound(item);
		try
		{
			m_SelectedEquipment.EquipWeapon((Equippable)item.baseItem, where.EquipmentSlot, m_Set);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception, this);
			return false;
		}
		UIInventoryManager.Instance.RefreshPartyMemberAppearance();
		UIInventoryManager.Instance.ReloadStats();
		GameState.PersistAcrossSceneLoadsTracked(item.baseItem);
		Persistence component = item.baseItem.GetComponent<Persistence>();
		if ((bool)component)
		{
			component.UnloadsBetweenLevels = false;
		}
		return true;
	}

	protected override Item DoRemove(Item item, int quantity)
	{
		throw new NotSupportedException("Equipment can't be in stacks.");
	}

	protected override int DoAdd(Item item, int quantity, UIInventoryGridItem where)
	{
		throw new NotSupportedException("Equipment can't be in stacks.");
	}

	private void TryPlayPutSound(InventoryItem item)
	{
		if (GlobalAudioPlayer.Instance != null)
		{
			GlobalAudioPlayer.Instance.Play(item.baseItem, GlobalAudioPlayer.UIInventoryAction.EquipItem);
		}
	}

	private void TryPlayTakeSound(InventoryItem item)
	{
		if (GlobalAudioPlayer.Instance != null && GlobalAudioPlayer.Instance.AllowPlayingOfTakeSound)
		{
			GlobalAudioPlayer.Instance.Play(item.baseItem, GlobalAudioPlayer.UIInventoryAction.PickUpItem);
		}
	}

	public void Init(int slot)
	{
		m_Set = slot;
		NumeralLabel.text = GUIUtils.GetText(230 + slot);
		PrimarySlot.Init();
		SecondarySlot.Init();
	}

	public void LoadWeaponSet(int index, WeaponSet ws)
	{
		BackingSet = ws;
		BackingSetIndex = index;
		Reload();
	}

	public void ClearClonesOf(UIInventoryGridItem set)
	{
		if (PrimarySlot.InvItem == set.InvItem)
		{
			PrimarySlot.ClearLink();
		}
		if (SecondarySlot.InvItem == set.InvItem)
		{
			SecondarySlot.ClearLink();
		}
	}

	public override void Reload()
	{
		if (BackingSet != null)
		{
			PrimarySlot.SetItem(new InventoryItem(BackingSet.PrimaryWeapon));
			SecondarySlot.SetItem(new InventoryItem(BackingSet.SecondaryWeapon));
		}
		else
		{
			PrimarySlot.UnsetItem();
			SecondarySlot.UnsetItem();
		}
	}

	public void ShowSelection()
	{
	}

	public void HideSelection()
	{
	}

	private void OnClick(GameObject sender)
	{
		UIInventoryWeaponSetManager uIInventoryWeaponSetManager = UnityEngine.Object.FindObjectOfType<UIInventoryWeaponSetManager>();
		if ((bool)uIInventoryWeaponSetManager)
		{
			uIInventoryWeaponSetManager.Select(BackingSetIndex);
		}
	}
}
