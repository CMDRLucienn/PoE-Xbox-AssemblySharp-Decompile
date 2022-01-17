using System;
using System.Collections;
using System.Collections.Generic;
using Polenter.Serialization;
using UnityEngine;

[Serializable]
public class EquipmentSet : IEnumerable<Equippable>, IEnumerable
{
	public delegate void WeaponSetChanged(WeaponSet newSet);

	public const int MAX_WEAPON_SETS = 4;

	private const float m_damaged_armor_DT_multiplier = 0.75f;

	public Equippable Head;

	public Equippable Neck;

	public Equippable Chest;

	public Equippable Hands;

	public Equippable RightHandRing;

	public Equippable LeftHandRing;

	[HideInInspector]
	public Equippable Cape;

	public Equippable Feet;

	public Equippable Waist;

	public Equippable Grimoire;

	public Equippable Pet;

	public Equippable PrimaryWeapon;

	public Equippable SecondaryWeapon;

	public WeaponSet[] AlternateWeaponSets = new WeaponSet[0];

	protected int m_SelectedWeaponSet;

	[NonSerialized]
	[ExcludeFromSerialization]
	[HideInInspector]
	public Ref<Equippable>[] Slots = new Ref<Equippable>[11];

	private Equippable m_pushedPrimaryWeapon;

	private Equippable m_pushedSecondaryWeapon;

	public int SelectedWeaponSet
	{
		get
		{
			return m_SelectedWeaponSet;
		}
		set
		{
			m_SelectedWeaponSet = value;
		}
	}

	public Equippable[] SerializedEquipment
	{
		get
		{
			return new Equippable[11]
			{
				Head, Neck, Chest, Hands, RightHandRing, LeftHandRing, Cape, Feet, Waist, Grimoire,
				Pet
			};
		}
		set
		{
			if (value != null)
			{
				Head = value[0];
				Neck = value[1];
				Chest = value[2];
				Hands = value[3];
				RightHandRing = value[4];
				LeftHandRing = value[5];
				Cape = value[6];
				Feet = value[7];
				Waist = value[8];
				Grimoire = value[9];
				Pet = value[10];
			}
		}
	}

	public bool Empty
	{
		get
		{
			Ref<Equippable>[] slots = Slots;
			for (int i = 0; i < slots.Length; i++)
			{
				if (slots[i].Val != null)
				{
					return false;
				}
			}
			WeaponSet[] alternateWeaponSets = AlternateWeaponSets;
			foreach (WeaponSet weaponSet in alternateWeaponSets)
			{
				if (weaponSet != null && !weaponSet.Empty())
				{
					return false;
				}
			}
			return true;
		}
	}

	public bool TwoHandedWeapon
	{
		get
		{
			if (PrimaryWeapon != null)
			{
				return PrimaryWeapon.BothPrimaryAndSecondarySlot;
			}
			return false;
		}
	}

	public bool DualWielding
	{
		get
		{
			if (PrimaryWeapon != null && SecondaryWeapon != null)
			{
				return true;
			}
			if (PrimaryWeapon == null)
			{
				return true;
			}
			return false;
		}
	}

	public Shield Shield
	{
		get
		{
			if (SecondaryWeapon != null)
			{
				Shield component = SecondaryWeapon.GetComponent<Shield>();
				if (component != null)
				{
					return component;
				}
			}
			if (PrimaryWeapon != null)
			{
				Shield component2 = PrimaryWeapon.GetComponent<Shield>();
				if (component2 != null)
				{
					return component2;
				}
			}
			return null;
		}
	}

	public Equippable PushedPrimaryWeapon
	{
		get
		{
			return m_pushedPrimaryWeapon;
		}
		set
		{
			m_pushedPrimaryWeapon = value;
		}
	}

	public Equippable PushedSecondaryWeapon
	{
		get
		{
			return m_pushedSecondaryWeapon;
		}
		set
		{
			m_pushedSecondaryWeapon = value;
		}
	}

	public event WeaponSetChanged OnWeaponSetChanged;

	public void SelectWeaponSet(GameObject owner, int index)
	{
		if (index < AlternateWeaponSets.Length)
		{
			WeaponSet selectedWeaponSet = GetSelectedWeaponSet();
			WeaponSet firstAlternateWeaponSet = GetFirstAlternateWeaponSet();
			m_SelectedWeaponSet = index;
			EquipWeaponSets(owner, selectedWeaponSet, firstAlternateWeaponSet);
			if (this.OnWeaponSetChanged != null)
			{
				this.OnWeaponSetChanged(AlternateWeaponSets[m_SelectedWeaponSet]);
			}
		}
	}

	public void SelectWeaponSet(GameObject owner, WeaponSet ws)
	{
		for (int i = 0; i < AlternateWeaponSets.Length; i++)
		{
			if (AlternateWeaponSets[i] == ws)
			{
				SelectWeaponSet(owner, i);
				break;
			}
		}
	}

	public IEnumerator<Equippable> GetEnumerator()
	{
		yield return Head;
		yield return Neck;
		yield return Chest;
		yield return Hands;
		yield return RightHandRing;
		yield return LeftHandRing;
		yield return Feet;
		yield return Waist;
		yield return Grimoire;
		yield return Pet;
		WeaponSet[] alternateWeaponSets = AlternateWeaponSets;
		foreach (WeaponSet weaponSet in alternateWeaponSets)
		{
			foreach (Equippable item in weaponSet)
			{
				yield return item;
			}
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public EquipmentSet()
	{
		Slots[0] = new Ref<Equippable>(() => Head, delegate(Equippable x)
		{
			Head = x;
		});
		Slots[1] = new Ref<Equippable>(() => Neck, delegate(Equippable x)
		{
			Neck = x;
		});
		Slots[2] = new Ref<Equippable>(() => Chest, delegate(Equippable x)
		{
			Chest = x;
		});
		Slots[3] = new Ref<Equippable>(() => RightHandRing, delegate(Equippable x)
		{
			RightHandRing = x;
		});
		Slots[4] = new Ref<Equippable>(() => LeftHandRing, delegate(Equippable x)
		{
			LeftHandRing = x;
		});
		Slots[5] = new Ref<Equippable>(() => Hands, delegate(Equippable x)
		{
			Hands = x;
		});
		Slots[6] = new Ref<Equippable>(() => null, delegate
		{
		});
		Slots[7] = new Ref<Equippable>(() => Feet, delegate(Equippable x)
		{
			Feet = x;
		});
		Slots[8] = new Ref<Equippable>(() => Waist, delegate(Equippable x)
		{
			Waist = x;
		});
		Slots[9] = new Ref<Equippable>(() => Grimoire, delegate(Equippable x)
		{
			Grimoire = x;
		});
		Slots[10] = new Ref<Equippable>(() => Pet, delegate(Equippable x)
		{
			Pet = x;
		});
	}

	public void PostDeserialized(GameObject owner)
	{
		if (AlternateWeaponSets.Length >= 4)
		{
			WeaponSet[] alternateWeaponSets = AlternateWeaponSets;
			AlternateWeaponSets = new WeaponSet[3];
			for (int i = 0; i < 3; i++)
			{
				AlternateWeaponSets[i] = alternateWeaponSets[i];
			}
		}
		WeaponSet[] array = new WeaponSet[4];
		AlternateWeaponSets.CopyTo(array, 1);
		array[0] = new WeaponSet(PrimaryWeapon, SecondaryWeapon);
		AlternateWeaponSets = array;
		for (int j = 0; j < AlternateWeaponSets.Length; j++)
		{
			if (AlternateWeaponSets[j] == null)
			{
				AlternateWeaponSets[j] = new WeaponSet();
			}
		}
	}

	public WeaponSet GetWeaponSet(int index)
	{
		if (index < AlternateWeaponSets.Length)
		{
			return AlternateWeaponSets[index];
		}
		return null;
	}

	public WeaponSet GetSelectedWeaponSet()
	{
		if (m_SelectedWeaponSet < AlternateWeaponSets.Length)
		{
			return AlternateWeaponSets[m_SelectedWeaponSet];
		}
		return null;
	}

	public WeaponSet GetFirstAlternateWeaponSet()
	{
		for (int i = 0; i < AlternateWeaponSets.Length; i++)
		{
			if (i != m_SelectedWeaponSet && AlternateWeaponSets[i] != null)
			{
				return AlternateWeaponSets[i];
			}
		}
		return null;
	}

	public int GetFirstAlternateWeaponSetIndex()
	{
		for (int i = 0; i < AlternateWeaponSets.Length; i++)
		{
			if (i != m_SelectedWeaponSet && AlternateWeaponSets[i] != null)
			{
				return i;
			}
		}
		return -1;
	}

	public void Validate(GameObject owner)
	{
		for (int i = 0; i < Slots.Length; i++)
		{
			if (Slots[i].Val != null && !Slots[i].Val.CanUseSlot((Equippable.EquipmentSlot)i))
			{
				UIDebug.Instance.LogOnScreenWarning(StringUtility.Format("Item '{1}' assigned to {0} slot on character '{2}' but it is not marked for that slot. It has been removed.", (Equippable.EquipmentSlot)i, Slots[i].Val.name, owner.name), UIDebug.Department.Design, 10f);
				Slots[i].Val = null;
			}
		}
		WeaponSet[] alternateWeaponSets = AlternateWeaponSets;
		foreach (WeaponSet weaponSet in alternateWeaponSets)
		{
			if (weaponSet != null)
			{
				if (weaponSet.PrimaryWeapon != null && !weaponSet.PrimaryWeapon.PrimaryWeaponSlot)
				{
					UIDebug.Instance.LogOnScreenWarning(StringUtility.Format("Item '{0}' assigned to Primary Weapon Set slot on character '{1}' but it is not marked for that slot. It has been removed.", weaponSet.PrimaryWeapon.name, owner.name), UIDebug.Department.Design, 10f);
					weaponSet.PrimaryWeapon = null;
				}
				if (weaponSet.SecondaryWeapon != null && !weaponSet.SecondaryWeapon.SecondaryWeaponSlot)
				{
					UIDebug.Instance.LogOnScreenWarning(StringUtility.Format("Item '{0}' assigned to Secondary Weapon Set slot on character '{1}' but it is not marked for that slot. It has been removed.", weaponSet.SecondaryWeapon.name, owner.name), UIDebug.Department.Design, 10f);
					weaponSet.SecondaryWeapon = null;
				}
			}
		}
	}

	public void EquipAllItems(GameObject owner)
	{
		if (!(owner != null))
		{
			return;
		}
		Equipment component = owner.GetComponent<Equipment>();
		if (!(component != null))
		{
			return;
		}
		for (int i = 0; i < Slots.Length; i++)
		{
			if (Slots[i].Val != null)
			{
				component.Equip(Slots[i].Val, (Equippable.EquipmentSlot)i, enforceRecoveryPenalty: false);
			}
		}
		WeaponSet selectedWeaponSet = GetSelectedWeaponSet();
		WeaponSet firstAlternateWeaponSet = GetFirstAlternateWeaponSet();
		int firstAlternateWeaponSetIndex = GetFirstAlternateWeaponSetIndex();
		if (selectedWeaponSet != null)
		{
			if ((bool)selectedWeaponSet.PrimaryWeapon)
			{
				component.EquipWeapon(selectedWeaponSet.PrimaryWeapon, Equippable.EquipmentSlot.PrimaryWeapon, m_SelectedWeaponSet);
			}
			if ((bool)selectedWeaponSet.SecondaryWeapon)
			{
				component.EquipWeapon(selectedWeaponSet.SecondaryWeapon, Equippable.EquipmentSlot.SecondaryWeapon, m_SelectedWeaponSet);
			}
		}
		if (firstAlternateWeaponSet != null && firstAlternateWeaponSetIndex >= 0)
		{
			if ((bool)firstAlternateWeaponSet.PrimaryWeapon)
			{
				component.EquipWeapon(firstAlternateWeaponSet.PrimaryWeapon, Equippable.EquipmentSlot.PrimaryWeapon2, firstAlternateWeaponSetIndex);
			}
			if ((bool)firstAlternateWeaponSet.SecondaryWeapon)
			{
				component.EquipWeapon(firstAlternateWeaponSet.SecondaryWeapon, Equippable.EquipmentSlot.SecondaryWeapon2, firstAlternateWeaponSetIndex);
			}
		}
	}

	public void EquipWeaponSets(GameObject owner, WeaponSet lastSet, WeaponSet lastAlternate)
	{
		if (m_SelectedWeaponSet >= AlternateWeaponSets.Length)
		{
			m_SelectedWeaponSet = 0;
			if (this.OnWeaponSetChanged != null)
			{
				this.OnWeaponSetChanged(AlternateWeaponSets[m_SelectedWeaponSet]);
			}
		}
		if (m_SelectedWeaponSet < AlternateWeaponSets.Length)
		{
			PrimaryWeapon = AlternateWeaponSets[m_SelectedWeaponSet].PrimaryWeapon;
			SecondaryWeapon = AlternateWeaponSets[m_SelectedWeaponSet].SecondaryWeapon;
		}
		WeaponSet selectedWeaponSet = GetSelectedWeaponSet();
		WeaponSet firstAlternateWeaponSet = GetFirstAlternateWeaponSet();
		bool num = lastAlternate == null || firstAlternateWeaponSet == null || lastAlternate.PrimaryWeapon != firstAlternateWeaponSet.PrimaryWeapon;
		bool flag = lastAlternate == null || firstAlternateWeaponSet == null || lastAlternate.SecondaryWeapon != firstAlternateWeaponSet.SecondaryWeapon;
		bool flag2 = lastSet == null || lastSet.PrimaryWeapon != selectedWeaponSet.PrimaryWeapon;
		bool flag3 = lastSet == null || lastSet.SecondaryWeapon != selectedWeaponSet.SecondaryWeapon;
		if (num && lastAlternate != null && lastAlternate.PrimaryWeapon != null)
		{
			lastAlternate.PrimaryWeapon.UnEquip(owner, Equippable.EquipmentSlot.PrimaryWeapon2);
		}
		if (flag && lastAlternate != null && lastAlternate.SecondaryWeapon != null)
		{
			lastAlternate.SecondaryWeapon.UnEquip(owner, Equippable.EquipmentSlot.SecondaryWeapon2);
		}
		if (flag2 && lastSet != null && lastSet.PrimaryWeapon != null)
		{
			lastSet.PrimaryWeapon.UnEquip(owner, Equippable.EquipmentSlot.PrimaryWeapon);
		}
		if (flag3 && lastSet != null && lastSet.SecondaryWeapon != null)
		{
			lastSet.SecondaryWeapon.UnEquip(owner, Equippable.EquipmentSlot.SecondaryWeapon);
		}
		if (num && firstAlternateWeaponSet != null && firstAlternateWeaponSet.PrimaryWeapon != null)
		{
			firstAlternateWeaponSet.PrimaryWeapon.Equip(owner, Equippable.EquipmentSlot.PrimaryWeapon2);
		}
		if (flag && firstAlternateWeaponSet != null && firstAlternateWeaponSet.SecondaryWeapon != null)
		{
			firstAlternateWeaponSet.SecondaryWeapon.Equip(owner, Equippable.EquipmentSlot.SecondaryWeapon2);
		}
		if (flag2 && selectedWeaponSet != null && selectedWeaponSet.PrimaryWeapon != null)
		{
			selectedWeaponSet.PrimaryWeapon.Equip(owner, Equippable.EquipmentSlot.PrimaryWeapon);
		}
		if (flag3 && selectedWeaponSet != null && selectedWeaponSet.SecondaryWeapon != null)
		{
			selectedWeaponSet.SecondaryWeapon.Equip(owner, Equippable.EquipmentSlot.SecondaryWeapon);
		}
	}

	public void InstantiateFromSet(GameObject owner, EquipmentSet set)
	{
		bool flag = false;
		Persistence component = owner.GetComponent<Persistence>();
		if (component != null && !component.UnloadsBetweenLevels)
		{
			flag = true;
		}
		for (int i = 0; i < set.Slots.Length; i++)
		{
			if (!(set.Slots[i].Val != null))
			{
				continue;
			}
			if (set.Slots[i].Val.IsPrefab)
			{
				Slots[i].Val = GameResources.Instantiate<Equippable>(set.Slots[i].Val);
				Slots[i].Val.Prefab = set.Slots[i].Val;
			}
			else
			{
				Slots[i].Val = set.Slots[i].Val;
			}
			if (flag)
			{
				GameState.PersistAcrossSceneLoadsTracked(Slots[i].Val);
				Persistence component2 = Slots[i].Val.GetComponent<Persistence>();
				if ((bool)component2)
				{
					component2.UnloadsBetweenLevels = false;
				}
			}
		}
		for (int j = 0; j < set.AlternateWeaponSets.Length && j < AlternateWeaponSets.Length; j++)
		{
			if (set.AlternateWeaponSets[j] == null)
			{
				continue;
			}
			Equippable primaryWeapon = set.AlternateWeaponSets[j].PrimaryWeapon;
			if (primaryWeapon != null)
			{
				if (primaryWeapon.IsPrefab)
				{
					AlternateWeaponSets[j].PrimaryWeapon = GameResources.Instantiate<Equippable>(primaryWeapon);
					AlternateWeaponSets[j].PrimaryWeapon.Prefab = primaryWeapon;
				}
				else
				{
					AlternateWeaponSets[j].PrimaryWeapon = primaryWeapon;
				}
				if (flag)
				{
					GameState.PersistAcrossSceneLoadsTracked(AlternateWeaponSets[j].PrimaryWeapon);
					Persistence component3 = AlternateWeaponSets[j].PrimaryWeapon.GetComponent<Persistence>();
					if ((bool)component3)
					{
						component3.UnloadsBetweenLevels = false;
					}
				}
			}
			Equippable secondaryWeapon = set.AlternateWeaponSets[j].SecondaryWeapon;
			if (!(secondaryWeapon != null))
			{
				continue;
			}
			if (secondaryWeapon.IsPrefab)
			{
				AlternateWeaponSets[j].SecondaryWeapon = GameResources.Instantiate<Equippable>(secondaryWeapon);
				AlternateWeaponSets[j].SecondaryWeapon.Prefab = secondaryWeapon;
			}
			else
			{
				AlternateWeaponSets[j].SecondaryWeapon = secondaryWeapon;
			}
			if (flag)
			{
				GameState.PersistAcrossSceneLoadsTracked(AlternateWeaponSets[j].SecondaryWeapon);
				Persistence component4 = AlternateWeaponSets[j].SecondaryWeapon.GetComponent<Persistence>();
				if ((bool)component4)
				{
					component4.UnloadsBetweenLevels = false;
				}
			}
		}
	}

	public void InstantiateAllItems()
	{
		Ref<Equippable>[] slots = Slots;
		foreach (Ref<Equippable> @ref in slots)
		{
			if (@ref.Val != null)
			{
				Equippable val = @ref.Val;
				@ref.Val = GameResources.Instantiate<Equippable>(@ref.Val);
				@ref.Val.Prefab = val;
				GameState.PersistAcrossSceneLoadsTracked(@ref.Val);
				Persistence component = @ref.Val.GetComponent<Persistence>();
				if ((bool)component)
				{
					component.UnloadsBetweenLevels = false;
				}
			}
		}
		WeaponSet[] alternateWeaponSets = AlternateWeaponSets;
		foreach (WeaponSet weaponSet in alternateWeaponSets)
		{
			if (weaponSet == null)
			{
				continue;
			}
			if (weaponSet.PrimaryWeapon != null)
			{
				Equippable primaryWeapon = weaponSet.PrimaryWeapon;
				weaponSet.PrimaryWeapon = GameResources.Instantiate<Equippable>(weaponSet.PrimaryWeapon);
				weaponSet.PrimaryWeapon.Prefab = primaryWeapon;
				GameState.PersistAcrossSceneLoadsTracked(weaponSet.PrimaryWeapon);
				Persistence component2 = weaponSet.PrimaryWeapon.GetComponent<Persistence>();
				if ((bool)component2)
				{
					component2.UnloadsBetweenLevels = false;
				}
			}
			if (weaponSet.SecondaryWeapon != null)
			{
				Equippable secondaryWeapon = weaponSet.SecondaryWeapon;
				weaponSet.SecondaryWeapon = GameResources.Instantiate<Equippable>(weaponSet.SecondaryWeapon);
				weaponSet.SecondaryWeapon.Prefab = secondaryWeapon;
				GameState.PersistAcrossSceneLoadsTracked(weaponSet.SecondaryWeapon);
				Persistence component3 = weaponSet.SecondaryWeapon.GetComponent<Persistence>();
				if ((bool)component3)
				{
					component3.UnloadsBetweenLevels = false;
				}
			}
		}
	}

	public Equippable GetItemInSlot(Equippable.EquipmentSlot slot)
	{
		if ((int)slot < Slots.Length)
		{
			return Slots[(int)slot].Val;
		}
		return slot switch
		{
			Equippable.EquipmentSlot.PrimaryWeapon => GetSelectedWeaponSet()?.PrimaryWeapon, 
			Equippable.EquipmentSlot.SecondaryWeapon => GetSelectedWeaponSet()?.SecondaryWeapon, 
			Equippable.EquipmentSlot.PrimaryWeapon2 => GetFirstAlternateWeaponSet()?.PrimaryWeapon, 
			Equippable.EquipmentSlot.SecondaryWeapon2 => GetFirstAlternateWeaponSet()?.SecondaryWeapon, 
			_ => null, 
		};
	}

	public Equippable.EquipmentSlot GetSlot(Equippable item)
	{
		for (int i = 0; i < Slots.Length; i++)
		{
			if (item == Slots[i].Val)
			{
				return (Equippable.EquipmentSlot)i;
			}
		}
		WeaponSet selectedWeaponSet = GetSelectedWeaponSet();
		if (selectedWeaponSet != null)
		{
			if (selectedWeaponSet.PrimaryWeapon == item)
			{
				return Equippable.EquipmentSlot.PrimaryWeapon;
			}
			if (selectedWeaponSet.SecondaryWeapon == item)
			{
				return Equippable.EquipmentSlot.SecondaryWeapon;
			}
		}
		WeaponSet[] alternateWeaponSets = AlternateWeaponSets;
		foreach (WeaponSet weaponSet in alternateWeaponSets)
		{
			if (weaponSet.PrimaryWeapon == item)
			{
				return Equippable.EquipmentSlot.PrimaryWeapon2;
			}
			if (weaponSet.SecondaryWeapon == item)
			{
				return Equippable.EquipmentSlot.SecondaryWeapon2;
			}
		}
		return Equippable.EquipmentSlot.None;
	}

	public Equippable.EquipmentSlot FindSlot(string itemName)
	{
		for (int i = 0; i < Slots.Length; i++)
		{
			if (Slots[i].Val != null && Slots[i].Val.NameEquals(itemName))
			{
				return (Equippable.EquipmentSlot)i;
			}
		}
		WeaponSet selectedWeaponSet = GetSelectedWeaponSet();
		if (selectedWeaponSet != null)
		{
			if (selectedWeaponSet.PrimaryWeapon != null && selectedWeaponSet.PrimaryWeapon.NameEquals(itemName))
			{
				return Equippable.EquipmentSlot.PrimaryWeapon;
			}
			if (selectedWeaponSet.SecondaryWeapon != null && selectedWeaponSet.SecondaryWeapon.NameEquals(itemName))
			{
				return Equippable.EquipmentSlot.SecondaryWeapon;
			}
		}
		WeaponSet[] alternateWeaponSets = AlternateWeaponSets;
		foreach (WeaponSet weaponSet in alternateWeaponSets)
		{
			if (weaponSet.PrimaryWeapon != null && weaponSet.PrimaryWeapon.NameEquals(itemName))
			{
				return Equippable.EquipmentSlot.PrimaryWeapon2;
			}
			if (weaponSet.SecondaryWeapon != null && weaponSet.SecondaryWeapon.NameEquals(itemName))
			{
				return Equippable.EquipmentSlot.SecondaryWeapon2;
			}
		}
		return Equippable.EquipmentSlot.None;
	}

	public void EquipItemToSlot(GameObject owner, Equippable item, Equippable.EquipmentSlot slot)
	{
		switch (slot)
		{
		case Equippable.EquipmentSlot.PrimaryWeapon2:
		case Equippable.EquipmentSlot.SecondaryWeapon2:
			Debug.LogError("Can't equip alternate weapons with EquipItemToSlot - use EquipAlternateWeapon instead.");
			return;
		case Equippable.EquipmentSlot.PrimaryWeapon:
		case Equippable.EquipmentSlot.SecondaryWeapon:
			EquipWeaponToSlot(owner, item, slot, 0);
			return;
		case Equippable.EquipmentSlot.None:
			return;
		}
		if ((int)slot < Slots.Length)
		{
			Slots[(int)slot].Val = item;
		}
		if ((bool)item)
		{
			item.Equip(owner, slot);
		}
	}

	public Equippable GetWeaponInSlot(Equippable.EquipmentSlot slot, int weaponIndex)
	{
		if (weaponIndex >= AlternateWeaponSets.Length)
		{
			return null;
		}
		return AlternateWeaponSets[weaponIndex].GetItemInSlot(slot);
	}

	public int GetWeaponSetIndexForItem(Equippable equippable)
	{
		for (int i = 0; i < AlternateWeaponSets.Length; i++)
		{
			if (!WeaponSet.IsNullOrEmpty(AlternateWeaponSets[i]) && (AlternateWeaponSets[i].PrimaryWeapon == equippable || AlternateWeaponSets[i].SecondaryWeapon == equippable))
			{
				return i;
			}
		}
		return -1;
	}

	public void EquipWeaponToSlot(GameObject owner, Equippable item, Equippable.EquipmentSlot slot, int weaponSetNumber)
	{
		if (slot == Equippable.EquipmentSlot.PrimaryWeapon2 || slot == Equippable.EquipmentSlot.PrimaryWeapon || slot == Equippable.EquipmentSlot.SecondaryWeapon2 || slot == Equippable.EquipmentSlot.SecondaryWeapon)
		{
			if (weaponSetNumber >= AlternateWeaponSets.Length)
			{
				Debug.LogError("Tried to equip " + item.gameObject.name + " to " + owner.gameObject.name + " weapon set " + weaponSetNumber + " (out of bounds).");
				return;
			}
			WeaponSet weaponSet = AlternateWeaponSets[weaponSetNumber];
			int firstAlternateWeaponSetIndex = GetFirstAlternateWeaponSetIndex();
			switch (slot)
			{
			case Equippable.EquipmentSlot.PrimaryWeapon:
			case Equippable.EquipmentSlot.PrimaryWeapon2:
				if (item != null)
				{
					if ((bool)owner)
					{
						item.transform.parent = owner.transform;
					}
					if (weaponSetNumber == SelectedWeaponSet)
					{
						item.Equip(owner, Equippable.EquipmentSlot.PrimaryWeapon);
						PrimaryWeapon = item;
					}
					else if (weaponSetNumber == firstAlternateWeaponSetIndex)
					{
						item.Equip(owner, Equippable.EquipmentSlot.PrimaryWeapon2);
					}
				}
				else if (weaponSetNumber == SelectedWeaponSet)
				{
					weaponSet.PrimaryWeapon.UnEquip(owner, Equippable.EquipmentSlot.PrimaryWeapon);
					PrimaryWeapon = null;
				}
				else if (weaponSetNumber == firstAlternateWeaponSetIndex)
				{
					weaponSet.PrimaryWeapon.UnEquip(owner, Equippable.EquipmentSlot.PrimaryWeapon2);
				}
				weaponSet.PrimaryWeapon = item;
				break;
			case Equippable.EquipmentSlot.SecondaryWeapon:
			case Equippable.EquipmentSlot.SecondaryWeapon2:
				if (item != null)
				{
					if ((bool)owner)
					{
						item.transform.parent = owner.transform;
					}
					if (weaponSetNumber == SelectedWeaponSet)
					{
						item.Equip(owner, Equippable.EquipmentSlot.SecondaryWeapon);
						SecondaryWeapon = item;
					}
					else if (weaponSetNumber == firstAlternateWeaponSetIndex)
					{
						item.Equip(owner, Equippable.EquipmentSlot.SecondaryWeapon2);
					}
				}
				else if (weaponSetNumber == SelectedWeaponSet)
				{
					weaponSet.SecondaryWeapon.UnEquip(owner, Equippable.EquipmentSlot.SecondaryWeapon);
					SecondaryWeapon = null;
				}
				else if (weaponSetNumber == firstAlternateWeaponSetIndex)
				{
					weaponSet.SecondaryWeapon.UnEquip(owner, Equippable.EquipmentSlot.SecondaryWeapon2);
				}
				weaponSet.SecondaryWeapon = item;
				break;
			}
		}
		else
		{
			EquipItemToSlot(owner, item, slot);
		}
	}

	public Equippable.EquipmentSlot GetDesiredSlot(Equippable item)
	{
		if (item.PrimaryWeaponSlot && item.SecondaryWeaponSlot)
		{
			if (PrimaryWeapon != null && SecondaryWeapon == null)
			{
				return Equippable.EquipmentSlot.SecondaryWeapon;
			}
			return Equippable.EquipmentSlot.PrimaryWeapon;
		}
		if (item.RingRightHandSlot && item.RingLeftHandSlot)
		{
			if (RightHandRing != null && LeftHandRing == null)
			{
				return Equippable.EquipmentSlot.LeftRing;
			}
			return Equippable.EquipmentSlot.RightRing;
		}
		if (item.Slots != null)
		{
			for (int i = 0; i < item.Slots.Length; i++)
			{
				if (item.Slots[i].Val)
				{
					return (Equippable.EquipmentSlot)i;
				}
			}
		}
		WeaponSet[] alternateWeaponSets = AlternateWeaponSets;
		foreach (WeaponSet weaponSet in alternateWeaponSets)
		{
			if (weaponSet != null)
			{
				if (weaponSet.PrimaryWeapon == item)
				{
					return Equippable.EquipmentSlot.PrimaryWeapon2;
				}
				if (weaponSet.SecondaryWeapon == item)
				{
					return Equippable.EquipmentSlot.SecondaryWeapon2;
				}
			}
		}
		return Equippable.EquipmentSlot.None;
	}

	public float CalculateDT(DamagePacket.DamageType dmgType, float bonusDT, GameObject wearer)
	{
		float num = 0f;
		float num2 = 0f;
		for (Equippable.EquipmentSlot equipmentSlot = Equippable.EquipmentSlot.Head; equipmentSlot < Equippable.EquipmentSlot.Count; equipmentSlot++)
		{
			Equippable itemInSlot = GetItemInSlot(equipmentSlot);
			if (!(itemInSlot != null))
			{
				continue;
			}
			Armor component = itemInSlot.GetComponent<Armor>();
			if (!(component != null))
			{
				continue;
			}
			float num3 = component.CalculateDT(dmgType, bonusDT, wearer);
			switch (equipmentSlot)
			{
			case Equippable.EquipmentSlot.PrimaryWeapon:
			case Equippable.EquipmentSlot.SecondaryWeapon:
				if (num3 > num2)
				{
					num2 = num3;
				}
				continue;
			case Equippable.EquipmentSlot.Armor:
				if (itemInSlot.DurabilityState == Equippable.DurabilityStateType.Damaged)
				{
					num3 *= 0.75f;
				}
				break;
			}
			if (num3 > num)
			{
				num = num3;
			}
		}
		return num + num2;
	}

	public float CalculateDR(DamagePacket.DamageType dmgType)
	{
		float num = 0f;
		float num2 = 0f;
		for (Equippable.EquipmentSlot equipmentSlot = Equippable.EquipmentSlot.Head; equipmentSlot < Equippable.EquipmentSlot.Count; equipmentSlot++)
		{
			Equippable itemInSlot = GetItemInSlot(equipmentSlot);
			if (!(itemInSlot != null))
			{
				continue;
			}
			Armor component = itemInSlot.GetComponent<Armor>();
			if (!(component != null))
			{
				continue;
			}
			float num3 = component.CalculateDR(dmgType);
			switch (equipmentSlot)
			{
			case Equippable.EquipmentSlot.PrimaryWeapon:
			case Equippable.EquipmentSlot.SecondaryWeapon:
				if (num3 > num2)
				{
					num2 = num3;
				}
				continue;
			case Equippable.EquipmentSlot.Armor:
				if (itemInSlot.DurabilityState == Equippable.DurabilityStateType.Damaged)
				{
					num3 *= 0.75f;
				}
				break;
			}
			if (num3 > num)
			{
				num = num3;
			}
		}
		return num + num2;
	}

	public List<AppearancePiece> GetAppearancePieces()
	{
		List<AppearancePiece> list = new List<AppearancePiece>();
		if (Chest != null)
		{
			list.Add(Chest.Appearance);
		}
		if (Head != null)
		{
			list.Add(Head.Appearance);
		}
		return list;
	}
}
