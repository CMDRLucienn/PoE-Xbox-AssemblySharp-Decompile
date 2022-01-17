using System;
using UnityEngine;

public class UIInventoryWeaponSetManager : UIParentSelectorListener
{
	private UIInventoryWeaponSet[] m_WeaponSets;

	public int MaxWeaponSets = 4;

	public UIInventoryWeaponSet Root;

	public UIGrid Grid;

	private Equipment m_SelectedEquipment;

	public void Init()
	{
		if (m_WeaponSets == null)
		{
			m_WeaponSets = new UIInventoryWeaponSet[MaxWeaponSets];
			for (int i = 0; i < MaxWeaponSets; i++)
			{
				m_WeaponSets[i] = NGUITools.AddChild(Root.transform.parent.gameObject, Root.gameObject).GetComponent<UIInventoryWeaponSet>();
				m_WeaponSets[i].gameObject.name = "WeaponSet" + i;
				m_WeaponSets[i].Init(i);
			}
			GameUtilities.Destroy(Root.gameObject);
			Grid.Reposition();
		}
	}

	public override void NotifySelectionChanged(CharacterStats selected)
	{
		if (selected == null)
		{
			throw new ArgumentNullException("selected");
		}
		if (m_WeaponSets == null)
		{
			return;
		}
		m_SelectedEquipment = selected.GetComponent<Equipment>();
		UIInventoryWeaponSet[] weaponSets = m_WeaponSets;
		foreach (UIInventoryWeaponSet uIInventoryWeaponSet in weaponSets)
		{
			if (uIInventoryWeaponSet != null)
			{
				uIInventoryWeaponSet.Load(selected.gameObject);
			}
		}
		ReloadInventory();
	}

	public void ClearClonesOf(UIInventoryGridItem item)
	{
		UIInventoryWeaponSet[] weaponSets = m_WeaponSets;
		for (int i = 0; i < weaponSets.Length; i++)
		{
			weaponSets[i].ClearClonesOf(item);
		}
	}

	public void ReloadInventory()
	{
		Init();
		for (int i = 0; i < m_WeaponSets.Length; i++)
		{
			if (i < m_SelectedEquipment.CurrentItems.AlternateWeaponSets.Length)
			{
				m_WeaponSets[i].LoadWeaponSet(i, m_SelectedEquipment.CurrentItems.AlternateWeaponSets[i]);
			}
			else
			{
				m_WeaponSets[i].LoadWeaponSet(-1, null);
			}
		}
		Select(m_SelectedEquipment.CurrentItems.SelectedWeaponSet);
		int j = 0;
		CharacterStats component = m_SelectedEquipment.GetComponent<CharacterStats>();
		if ((bool)component)
		{
			for (; j < Mathf.Min(m_WeaponSets.Length, component.MaxWeaponSets); j++)
			{
				m_WeaponSets[j].Disabled = false;
			}
		}
		for (; j < m_WeaponSets.Length; j++)
		{
			m_WeaponSets[j].Disabled = true;
		}
	}

	public void Select(int set)
	{
		m_SelectedEquipment.SelectWeaponSet(set, enforceRecoveryPenalty: true);
		RefreshSelection();
	}

	public void RefreshSelection()
	{
		for (int i = 0; i < m_WeaponSets.Length; i++)
		{
			if (i == m_SelectedEquipment.CurrentItems.SelectedWeaponSet)
			{
				m_WeaponSets[i].ShowSelection();
			}
			else
			{
				m_WeaponSets[i].HideSelection();
			}
		}
		UIInventoryManager.Instance.ReloadStats();
		UIInventoryManager.Instance.ReloadPaperdoll();
	}
}
