using System;
using System.Collections.Generic;
using UnityEngine;

public class UIInventoryEquipment : UIInventoryItemZone
{
	private UIInventoryGridItem[] m_GridItems;

	private Equipment m_SelectedEquipment;

	public Equipment SelectedEquipment => m_SelectedEquipment;

	private void Awake()
	{
		UIWindowManager.FindParentISelectACharacter(base.transform).OnSelectedCharacterChanged += ReloadCharacter;
	}

	public void Init()
	{
		if (m_GridItems == null)
		{
			m_GridItems = GetComponentsInChildren<UIInventoryGridItem>(includeInactive: true);
		}
	}

	private void ReloadCharacter(CharacterStats selected)
	{
		UpdateSelectedCharacter(selected);
		Reload();
	}

	public void UpdateSelectedCharacter(MonoBehaviour selected)
	{
		m_SelectedEquipment = selected.GetComponent<Equipment>();
		base.OwnerGameObject = selected.gameObject;
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
			m_SelectedEquipment.UnEquip((Equippable)item.baseItem);
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
		if (!where)
		{
			Debug.LogError("UIInventoryEquipment requires where slot to be passed.");
			return false;
		}
		TryPlayPutSound(item);
		m_SelectedEquipment.Equip((Equippable)item.baseItem, where.EquipmentSlot, enforceRecoveryPenalty: true);
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

	public UIInventoryGridItem GetGridItemForSlot(Equippable.EquipmentSlot slot)
	{
		if (slot == Equippable.EquipmentSlot.RightRing || slot == Equippable.EquipmentSlot.LeftRing)
		{
			UIInventoryGridItem gridItemForExactSlot = GetGridItemForExactSlot(Equippable.EquipmentSlot.RightRing);
			UIInventoryGridItem gridItemForExactSlot2 = GetGridItemForExactSlot(Equippable.EquipmentSlot.LeftRing);
			if (!gridItemForExactSlot)
			{
				return gridItemForExactSlot2;
			}
			if (!gridItemForExactSlot2)
			{
				return gridItemForExactSlot;
			}
			if (!gridItemForExactSlot.Empty && gridItemForExactSlot2.Empty)
			{
				return gridItemForExactSlot2;
			}
			return gridItemForExactSlot;
		}
		return GetGridItemForExactSlot(slot);
	}

	public UIInventoryGridItem GetGridItemForExactSlot(Equippable.EquipmentSlot slot)
	{
		if (m_GridItems != null)
		{
			UIInventoryGridItem[] gridItems = m_GridItems;
			foreach (UIInventoryGridItem uIInventoryGridItem in gridItems)
			{
				if (uIInventoryGridItem.EquipmentSlot == slot)
				{
					return uIInventoryGridItem;
				}
			}
		}
		return null;
	}

	public IEnumerable<Item.UIEquippedItem> GetComparisonTargets(Equippable compareWith)
	{
		return GetComparisonTargets(compareWith, m_SelectedEquipment);
	}

	public static IEnumerable<Item.UIEquippedItem> GetComparisonTargets(Equippable compareWith, Equipment equipment)
	{
		List<Item.UIEquippedItem> list = new List<Item.UIEquippedItem>();
		if (!compareWith || !equipment)
		{
			return list;
		}
		if (Equippable.IsWeapon(compareWith.GetPreferredSlot()))
		{
			WeaponSet selectedWeaponSet = equipment.CurrentItems.GetSelectedWeaponSet();
			if (selectedWeaponSet == null)
			{
				return list;
			}
			Equippable primaryWeapon = selectedWeaponSet.PrimaryWeapon;
			if ((bool)primaryWeapon && compareWith.PrimaryWeaponSlot)
			{
				list.Add(new Item.UIEquippedItem(primaryWeapon));
			}
			primaryWeapon = selectedWeaponSet.SecondaryWeapon;
			if ((bool)primaryWeapon && compareWith.SecondaryWeaponSlot)
			{
				list.Add(new Item.UIEquippedItem(primaryWeapon));
			}
		}
		else
		{
			Equippable itemInSlot = equipment.CurrentItems.GetItemInSlot(compareWith.GetPreferredSlot());
			if ((bool)itemInSlot)
			{
				list.Add(new Item.UIEquippedItem(itemInSlot));
			}
		}
		return list;
	}

	public override void Reload()
	{
		if (m_GridItems == null)
		{
			return;
		}
		UIInventoryGridItem[] gridItems = m_GridItems;
		foreach (UIInventoryGridItem uIInventoryGridItem in gridItems)
		{
			if (uIInventoryGridItem != null)
			{
				if (!m_SelectedEquipment || !m_SelectedEquipment.HasEquipmentSlot(uIInventoryGridItem.EquipmentSlot))
				{
					uIInventoryGridItem.Block();
					uIInventoryGridItem.SetItem(null);
				}
				else if (uIInventoryGridItem.EquipmentSlot != Equippable.EquipmentSlot.None)
				{
					uIInventoryGridItem.Unblock();
					uIInventoryGridItem.SetItem(new InventoryItem(m_SelectedEquipment.CurrentItems.GetItemInSlot(uIInventoryGridItem.EquipmentSlot)));
				}
			}
		}
	}
}
