using UnityEngine;

public class MirrorEquipment : MonoBehaviour
{
	public Equipment m_parentEquipment;

	private Equipment m_equipmentComponent;

	private void Start()
	{
		m_equipmentComponent = GetComponent<Equipment>();
	}

	private void Update()
	{
		if (m_equipmentComponent == null || m_parentEquipment == null)
		{
			return;
		}
		EquipmentSet currentItems = m_parentEquipment.CurrentItems;
		EquipmentSet currentItems2 = m_equipmentComponent.CurrentItems;
		Equippable equippable = null;
		Equippable equippable2 = null;
		for (int i = 0; i < currentItems.Slots.Length; i++)
		{
			equippable = null;
			equippable2 = null;
			if (currentItems.Slots[i].Val != null)
			{
				equippable = currentItems.Slots[i].Val.Prefab as Equippable;
			}
			if (currentItems2.Slots[i].Val != null)
			{
				equippable2 = currentItems2.Slots[i].Val.Prefab as Equippable;
			}
			if (equippable != equippable2)
			{
				if (equippable == null)
				{
					m_equipmentComponent.Equip(null, (Equippable.EquipmentSlot)i, enforceRecoveryPenalty: false);
				}
				else
				{
					Equippable equippable3 = GameResources.Instantiate<Equippable>(equippable);
					equippable3.Prefab = equippable;
					m_equipmentComponent.Equip(equippable3, (Equippable.EquipmentSlot)i, enforceRecoveryPenalty: false);
				}
				NPCAppearance component = GetComponent<NPCAppearance>();
				if ((bool)component)
				{
					component.Generate();
				}
			}
		}
		equippable = null;
		equippable2 = null;
		if (currentItems.GetSelectedWeaponSet().PrimaryWeapon != null)
		{
			equippable = currentItems.GetSelectedWeaponSet().PrimaryWeapon.Prefab as Equippable;
		}
		if (currentItems2.GetSelectedWeaponSet().PrimaryWeapon != null)
		{
			equippable2 = currentItems2.GetSelectedWeaponSet().PrimaryWeapon.Prefab as Equippable;
		}
		if (equippable != equippable2)
		{
			if (equippable == null)
			{
				m_equipmentComponent.EquipWeapon(null, Equippable.EquipmentSlot.PrimaryWeapon, currentItems2.SelectedWeaponSet);
			}
			else
			{
				Equippable equippable4 = GameResources.Instantiate<Equippable>(equippable);
				equippable4.Prefab = equippable;
				m_equipmentComponent.EquipWeapon(equippable4, Equippable.EquipmentSlot.PrimaryWeapon, currentItems2.SelectedWeaponSet);
			}
		}
		equippable = null;
		equippable2 = null;
		if (currentItems.GetSelectedWeaponSet().SecondaryWeapon != null)
		{
			equippable = currentItems.GetSelectedWeaponSet().SecondaryWeapon.Prefab as Equippable;
		}
		if (currentItems2.GetSelectedWeaponSet().SecondaryWeapon != null)
		{
			equippable2 = currentItems2.GetSelectedWeaponSet().SecondaryWeapon.Prefab as Equippable;
		}
		if (equippable != equippable2)
		{
			if (equippable == null)
			{
				m_equipmentComponent.EquipWeapon(null, Equippable.EquipmentSlot.SecondaryWeapon, currentItems2.SelectedWeaponSet);
			}
			else
			{
				Equippable equippable5 = GameResources.Instantiate<Equippable>(equippable);
				equippable5.Prefab = equippable;
				m_equipmentComponent.EquipWeapon(equippable5, Equippable.EquipmentSlot.SecondaryWeapon, currentItems2.SelectedWeaponSet);
			}
		}
		equippable = null;
		equippable2 = null;
		if (currentItems.GetFirstAlternateWeaponSet() != null && currentItems.GetFirstAlternateWeaponSet().PrimaryWeapon != null)
		{
			equippable = currentItems.GetFirstAlternateWeaponSet().PrimaryWeapon.Prefab as Equippable;
		}
		if (currentItems2.GetFirstAlternateWeaponSet() != null && currentItems2.GetFirstAlternateWeaponSet().PrimaryWeapon != null)
		{
			equippable2 = currentItems2.GetFirstAlternateWeaponSet().PrimaryWeapon.Prefab as Equippable;
		}
		if (equippable != equippable2)
		{
			if (equippable == null)
			{
				m_equipmentComponent.EquipWeapon(null, Equippable.EquipmentSlot.PrimaryWeapon2, currentItems2.SelectedWeaponSet);
			}
			else
			{
				Equippable equippable6 = GameResources.Instantiate<Equippable>(equippable);
				equippable6.Prefab = equippable;
				m_equipmentComponent.EquipWeapon(equippable6, Equippable.EquipmentSlot.PrimaryWeapon2, currentItems2.SelectedWeaponSet);
			}
		}
		equippable = null;
		equippable2 = null;
		if (currentItems.GetFirstAlternateWeaponSet() != null && currentItems.GetFirstAlternateWeaponSet().SecondaryWeapon != null)
		{
			equippable = currentItems.GetFirstAlternateWeaponSet().SecondaryWeapon.Prefab as Equippable;
		}
		if (currentItems2.GetFirstAlternateWeaponSet() != null && currentItems2.GetFirstAlternateWeaponSet().SecondaryWeapon != null)
		{
			equippable2 = currentItems2.GetFirstAlternateWeaponSet().SecondaryWeapon.Prefab as Equippable;
		}
		if (equippable != equippable2)
		{
			if (equippable == null)
			{
				m_equipmentComponent.EquipWeapon(null, Equippable.EquipmentSlot.SecondaryWeapon2, currentItems2.SelectedWeaponSet);
				return;
			}
			Equippable equippable7 = GameResources.Instantiate<Equippable>(equippable);
			equippable7.Prefab = equippable;
			m_equipmentComponent.EquipWeapon(equippable7, Equippable.EquipmentSlot.SecondaryWeapon2, currentItems2.SelectedWeaponSet);
		}
	}
}
