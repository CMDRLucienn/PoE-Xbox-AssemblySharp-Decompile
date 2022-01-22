using System;
using System.Collections.Generic;
using UnityEngine;

public class Equipment : MonoBehaviour
{
	public delegate void EquipmentChanged(Equippable.EquipmentSlot slot, Equippable oldEq, Equippable newEq, bool swappingSummonedWeapon, bool enforceRecoveryPenalty);

	public EquipmentSet DefaultEquippedItems = new EquipmentSet();

	[HideInInspector]
	public EquipmentSet m_currentItems;

	private bool m_initalizedSet;

	private bool m_initalizingSet;

	private bool m_isSwappingSummonedWeapon;

	private Weapon m_primaryWeapon;

	private Weapon m_secondaryWeapon;

	private AttackBase m_primaryUnarmedAttack;

	private AttackBase m_secondaryUnarmedAttack;

	[Persistent]
	private bool[] m_SlotLocked = new bool[13];

	private bool m_hasDeserialized;

	[HideInInspector]
	public bool m_shouldSaveEquipment;

	public AttackBase PrimaryUnarmedAttack
	{
		get
		{
			if (m_primaryUnarmedAttack == null)
			{
				AttackBase[] components = GetComponents<AttackBase>();
				if (components != null && components.Length != 0)
				{
					m_primaryUnarmedAttack = GetComponents<AttackBase>()[0];
				}
			}
			return m_primaryUnarmedAttack;
		}
		set
		{
			m_primaryUnarmedAttack = value;
		}
	}

	public AttackBase SecondaryUnarmedAttack
	{
		get
		{
			if (m_secondaryUnarmedAttack == null && GetComponents<AttackBase>().Length >= 2)
			{
				m_secondaryUnarmedAttack = GetComponents<AttackBase>()[1];
			}
			return m_secondaryUnarmedAttack;
		}
		set
		{
			m_secondaryUnarmedAttack = value;
		}
	}

	public EquipmentSet CurrentItems
	{
		get
		{
			if (!m_initalizedSet && !m_hasDeserialized)
			{
				Persistence component = GetComponent<Persistence>();
				if (!GameState.IsRestoredLevel || (component != null && !component.Mobile))
				{
					DefaultEquippedItems.Validate(base.gameObject);
					InitializeSet();
				}
			}
			return m_currentItems;
		}
	}

	[Persistent(Persistent.ConversionType.GUIDLink)]
	public List<Equippable> EquipmentSetSerialized
	{
		get
		{
			List<Equippable> list = new List<Equippable>();
			if (m_shouldSaveEquipment)
			{
				list.AddRange(m_currentItems.SerializedEquipment);
			}
			return list;
		}
		set
		{
			if (!m_shouldSaveEquipment)
			{
				return;
			}
			for (int i = 0; i < value.Count; i++)
			{
				if (m_currentItems.SerializedEquipment[i] != null && value[i] != m_currentItems.SerializedEquipment[i])
				{
					GameUtilities.Destroy(m_currentItems.SerializedEquipment[i].gameObject);
				}
			}
			m_currentItems.SerializedEquipment = value.ToArray();
			m_hasDeserialized = true;
		}
	}

	public WeaponSet[] WeaponSets => m_currentItems.AlternateWeaponSets;

	public int NumWeaponSets
	{
		get
		{
			CharacterStats component = GetComponent<CharacterStats>();
			bool flag = false;
			int num = 0;
			WeaponSet[] weaponSets = WeaponSets;
			for (int i = 0; i < weaponSets.Length; i++)
			{
				bool flag2 = WeaponSet.IsNullOrEmpty(weaponSets[i]);
				if (!flag2 || (!flag && num < component.MaxWeaponSets))
				{
					if (flag2)
					{
						flag = true;
					}
					num++;
				}
			}
			return num;
		}
	}

	[Persistent(Persistent.ConversionType.GUIDLink)]
	public List<Equippable> WeaponSetsSerialized
	{
		get
		{
			List<Equippable> list = new List<Equippable>();
			if (m_shouldSaveEquipment)
			{
				WeaponSet[] alternateWeaponSets = m_currentItems.AlternateWeaponSets;
				foreach (WeaponSet weaponSet in alternateWeaponSets)
				{
					list.Add(weaponSet.PrimaryWeapon);
					list.Add(weaponSet.SecondaryWeapon);
				}
			}
			return list;
		}
		set
		{
			if (value == null || value.Count == 0 || !m_shouldSaveEquipment)
			{
				return;
			}
			m_currentItems.PrimaryWeapon = null;
			m_currentItems.SecondaryWeapon = null;
			int num = 0;
			m_currentItems.AlternateWeaponSets = new WeaponSet[value.Count / 2];
			for (int i = 0; i < m_currentItems.AlternateWeaponSets.Length; i++)
			{
				if (num >= value.Count)
				{
					break;
				}
				m_currentItems.AlternateWeaponSets[i] = new WeaponSet(value[num], value[num + 1]);
				num += 2;
			}
			m_hasDeserialized = true;
		}
	}

	[Persistent]
	public int SelectedWeaponSetSerialized
	{
		get
		{
			if (m_currentItems == null)
			{
				return 0;
			}
			return m_currentItems.SelectedWeaponSet;
		}
		set
		{
			if (m_shouldSaveEquipment && m_currentItems != null)
			{
				m_currentItems.SelectedWeaponSet = value;
			}
		}
	}

	public bool DualWielding
	{
		get
		{
			if (m_currentItems.PrimaryWeapon != null && m_currentItems.SecondaryWeapon != null && SecondaryAttack != null)
			{
				return true;
			}
			if (m_currentItems.PrimaryWeapon == null)
			{
				return true;
			}
			AttackBase primaryAttack = PrimaryAttack;
			if (primaryAttack != null && primaryAttack is AttackMelee && (primaryAttack as AttackMelee).Unarmed)
			{
				AttackBase secondaryAttack = SecondaryAttack;
				if (secondaryAttack == null)
				{
					return true;
				}
				if (secondaryAttack is AttackMelee && (secondaryAttack as AttackMelee).Unarmed)
				{
					return true;
				}
			}
			return false;
		}
	}

	public bool TwoHandedWeapon => m_currentItems.TwoHandedWeapon;

	public AttackBase PrimaryAttack
	{
		get
		{
			if (m_currentItems != null && m_currentItems.PrimaryWeapon != null)
			{
				AttackBase component = m_currentItems.PrimaryWeapon.GetComponent<AttackBase>();
				if (component != null)
				{
					return component;
				}
			}
			return PrimaryUnarmedAttack;
		}
	}

	public AttackBase SecondaryAttack
	{
		get
		{
			if (m_currentItems == null)
			{
				return null;
			}
			if (m_currentItems.SecondaryWeapon == null && m_currentItems.PrimaryWeapon == null)
			{
				return SecondaryUnarmedAttack;
			}
			if (m_currentItems.PrimaryWeapon != null && m_currentItems.SecondaryWeapon == null)
			{
				return null;
			}
			AttackBase component = m_currentItems.SecondaryWeapon.GetComponent<AttackBase>();
			if (component != null)
			{
				return component;
			}
			return m_currentItems.SecondaryWeapon.FindItemModSecondaryAttack();
		}
	}

	public Weapon PrimaryWeapon
	{
		get
		{
			if (m_currentItems == null)
			{
				return null;
			}
			if (m_primaryWeapon == null)
			{
				AttackBase primaryAttack = PrimaryAttack;
				if (primaryAttack != null)
				{
					m_primaryWeapon = primaryAttack.GetComponent<Weapon>();
				}
			}
			return m_primaryWeapon;
		}
	}

	public Weapon SecondaryWeapon
	{
		get
		{
			if (m_currentItems == null)
			{
				return null;
			}
			if (m_secondaryWeapon == null)
			{
				AttackBase secondaryAttack = SecondaryAttack;
				if (secondaryAttack != null)
				{
					m_secondaryWeapon = secondaryAttack.GetComponent<Weapon>();
				}
			}
			return m_secondaryWeapon;
		}
	}

	public Shield EquippedShield
	{
		get
		{
			if (m_currentItems == null)
			{
				return null;
			}
			return m_currentItems.Shield;
		}
	}

	public bool HasSummonedWeapon
	{
		get
		{
			Equippable itemInSlot = m_currentItems.GetItemInSlot(Equippable.EquipmentSlot.PrimaryWeapon);
			if ((bool)itemInSlot && itemInSlot.IsSummoned)
			{
				return true;
			}
			Equippable itemInSlot2 = m_currentItems.GetItemInSlot(Equippable.EquipmentSlot.SecondaryWeapon);
			if ((bool)itemInSlot2 && itemInSlot2.IsSummoned)
			{
				return true;
			}
			return false;
		}
	}

	public event EquipmentChanged OnEquipmentChanged;

	public void Awake()
	{
		Persistence component = GetComponent<Persistence>();
		if ((bool)component)
		{
			m_shouldSaveEquipment = component.Mobile;
		}
	}

	public void Start()
	{
		if (!m_hasDeserialized)
		{
			DefaultEquippedItems.Validate(base.gameObject);
			InitializeSet();
		}
		AnimationController component = GetComponent<AnimationController>();
		if (component != null)
		{
			component.OnEventShowSlot += HandleAnimShowSlot;
			component.OnEventHideSlot += HandleAnimHideSlot;
			component.OnEventMoveToHand += HandleAnimMoveToHand;
			component.OnEventMoveFromHand += HandleAnimMoveFromHand;
		}
	}

	private void InitializeSet()
	{
		if (!m_initalizedSet)
		{
			m_initalizedSet = true;
			m_initalizingSet = true;
			DefaultEquippedItems.PostDeserialized(base.gameObject);
			m_currentItems = new EquipmentSet();
			m_currentItems.PostDeserialized(base.gameObject);
			m_currentItems.InstantiateFromSet(base.gameObject, DefaultEquippedItems);
			m_currentItems.EquipAllItems(base.gameObject);
			m_initalizingSet = false;
		}
	}

	private void RepairSaveLoadEquipmentErrors()
	{
		if (m_SlotLocked == null || m_currentItems == null)
		{
			return;
		}
		WeaponSet[] alternateWeaponSets = CurrentItems.AlternateWeaponSets;
		foreach (WeaponSet weaponSet in alternateWeaponSets)
		{
			if (weaponSet == null)
			{
				continue;
			}
			foreach (Equippable item in weaponSet)
			{
				if ((bool)item && item.transform.parent == null)
				{
					item.transform.parent = base.transform;
				}
			}
		}
		if ((bool)CurrentItems.Cape)
		{
			Equippable.EquipmentSlot desiredSlot = CurrentItems.GetDesiredSlot(CurrentItems.Cape);
			Equippable equippable = CurrentItems.Cape;
			CurrentItems.Cape = null;
			if (desiredSlot != Equippable.EquipmentSlot.None && CurrentItems.GetItemInSlot(desiredSlot) == null)
			{
				equippable = Equip(equippable);
			}
			if ((bool)equippable)
			{
				Inventory component = GetComponent<Inventory>();
				if ((bool)component)
				{
					component.AddItem(equippable, 1);
				}
			}
		}
		for (int j = 0; j < m_SlotLocked.Length; j++)
		{
			if (!m_SlotLocked[j])
			{
				continue;
			}
			if ((bool)UIDebug.Instance)
			{
				UIDebug.Instance.LogOnScreenWarning("Equipment slot locked on " + base.gameObject.name + " after load! Attempting to repair save game!", UIDebug.Department.Programming, 10f);
			}
			m_SlotLocked[j] = false;
			if ((bool)m_currentItems.PrimaryWeapon)
			{
				Persistence component2 = m_currentItems.PrimaryWeapon.GetComponent<Persistence>();
				if ((bool)component2)
				{
					component2.SetForDestroy();
				}
				GameUtilities.Destroy(m_currentItems.PrimaryWeapon);
			}
			if ((bool)m_currentItems.SecondaryWeapon)
			{
				Persistence component3 = m_currentItems.SecondaryWeapon.GetComponent<Persistence>();
				if ((bool)component3)
				{
					component3.SetForDestroy();
				}
				GameUtilities.Destroy(m_currentItems.SecondaryWeapon);
			}
			if (m_currentItems.AlternateWeaponSets != null && m_currentItems.AlternateWeaponSets.Length != 0 && m_currentItems.AlternateWeaponSets[0] != null)
			{
				if (m_currentItems.AlternateWeaponSets[0].PrimaryWeapon != null)
				{
					Persistence component4 = m_currentItems.AlternateWeaponSets[0].PrimaryWeapon.GetComponent<Persistence>();
					if ((bool)component4)
					{
						component4.SetForDestroy();
					}
					GameUtilities.Destroy(m_currentItems.AlternateWeaponSets[0].PrimaryWeapon);
				}
				if (m_currentItems.AlternateWeaponSets[0].SecondaryWeapon != null)
				{
					Persistence component5 = m_currentItems.AlternateWeaponSets[0].SecondaryWeapon.GetComponent<Persistence>();
					if ((bool)component5)
					{
						component5.SetForDestroy();
					}
					GameUtilities.Destroy(m_currentItems.AlternateWeaponSets[0].SecondaryWeapon);
				}
			}
			if (j != 9 || !(m_currentItems.Grimoire == null))
			{
				continue;
			}
			CharacterStats component6 = GetComponent<CharacterStats>();
			if (!component6 || component6.CharacterClass != CharacterStats.Class.Wizard)
			{
				continue;
			}
			GameObject gameObject = GameResources.LoadPrefab<GameObject>("Empty_Grimoire_01", instantiate: true);
			if (!gameObject)
			{
				continue;
			}
			Equippable component7 = gameObject.GetComponent<Equippable>();
			if ((bool)component7)
			{
				Grimoire component8 = component7.GetComponent<Grimoire>();
				if ((bool)component8)
				{
					component8.PrimaryOwnerName = component6.Name();
					Equip(component7, Equippable.EquipmentSlot.Grimoire, enforceRecoveryPenalty: false);
				}
			}
		}
	}

	public void Restored()
	{
		if (!m_shouldSaveEquipment || !GameState.IsRestoredLevel)
		{
			return;
		}
		m_initalizedSet = m_initalizedSet || GameState.IsRestoredLevel;
		m_initalizingSet = true;
		m_currentItems.EquipAllItems(base.gameObject);
		m_initalizingSet = false;
		if (GameState.LoadedGame && GetComponent<Persistence>().Mobile)
		{
			NPCAppearance component = GetComponent<NPCAppearance>();
			if ((bool)component)
			{
				component.Generate();
			}
		}
		RepairSaveLoadEquipmentErrors();
	}

	public void LockSlot(Equippable.EquipmentSlot slot)
	{
		if ((int)slot < m_SlotLocked.Length)
		{
			m_SlotLocked[(int)slot] = true;
		}
	}

	public void UnlockSlot(Equippable.EquipmentSlot slot)
	{
		if ((int)slot < m_SlotLocked.Length)
		{
			m_SlotLocked[(int)slot] = false;
		}
	}

	public bool HasEquipmentSlot(Equippable.EquipmentSlot slot)
	{
		// Start of mod
		if (IEModOptions.AllInventorySlots)
		{
			return true;
		}
		//End of mod, rest is normal code
		CharacterStats component = GetComponent<CharacterStats>();
		switch (slot)
		{
		case Equippable.EquipmentSlot.Grimoire:
			if ((bool)component)
			{
				return component.CharacterClass == CharacterStats.Class.Wizard;
			}
			return true;
		case Equippable.EquipmentSlot.Head:
			if ((bool)component)
			{
				return component.CharacterRace != CharacterStats.Race.Godlike;
			}
			return true;
		case Equippable.EquipmentSlot.Pet:
			return GetComponent<Player>();
		default:
			return true;
		}
	}

	public bool WeaponSlotLocked()
	{
		if (!m_SlotLocked[11])
		{
			return m_SlotLocked[12];
		}
		return true;
	}

	public bool IsSlotLocked(Equippable.EquipmentSlot slot)
	{
		switch (slot)
		{
		case Equippable.EquipmentSlot.PrimaryWeapon2:
			slot = Equippable.EquipmentSlot.PrimaryWeapon;
			break;
		case Equippable.EquipmentSlot.SecondaryWeapon2:
			slot = Equippable.EquipmentSlot.SecondaryWeapon;
			break;
		}
		if ((int)slot < m_SlotLocked.Length)
		{
			return m_SlotLocked[(int)slot];
		}
		return false;
	}

	public bool ShouldGrimoireBeDisplayed()
	{
		if (CurrentItems.GetItemInSlot(Equippable.EquipmentSlot.Grimoire) == null)
		{
			return false;
		}
		if (SecondaryWeapon != null || EquippedShield != null)
		{
			return false;
		}
		AttackBase secondaryAttack = SecondaryAttack;
		if (secondaryAttack != null && secondaryAttack != SecondaryUnarmedAttack)
		{
			return false;
		}
		Weapon primaryWeapon = PrimaryWeapon;
		if (primaryWeapon != null && (primaryWeapon.AnimationStance != Weapon.Stance.OneHanded || primaryWeapon.WeaponType == WeaponSpecializationData.WeaponType.HuntingBow || primaryWeapon.WeaponType == WeaponSpecializationData.WeaponType.WarBow))
		{
			return false;
		}
		return true;
	}

	public bool CanChangeWeaponSets()
	{
		if (((bool)m_currentItems.PrimaryWeapon && !m_currentItems.PrimaryWeapon.CanUnequip()) || ((bool)m_currentItems.SecondaryWeapon && !m_currentItems.SecondaryWeapon.CanUnequip()))
		{
			return false;
		}
		return true;
	}

	public bool IsWeaponSetValid(int index)
	{
		if (!CanChangeWeaponSets())
		{
			return index == m_currentItems.SelectedWeaponSet;
		}
		return !WeaponSet.IsNullOrEmpty(m_currentItems.GetWeaponSet(index));
	}

	public Equippable GetPrimaryWeaponFromWeaponSet(int weaponSetIndex)
	{
		if (WeaponSlotLocked())
		{
			return null;
		}
		WeaponSet weaponSet = m_currentItems.GetWeaponSet(weaponSetIndex);
		if (weaponSet == null)
		{
			return null;
		}
		if (weaponSet.PrimaryWeapon != null)
		{
			return weaponSet.PrimaryWeapon;
		}
		return weaponSet.SecondaryWeapon;
	}

	public void SelectNextWeaponSet()
	{
		if (!CanChangeWeaponSets())
		{
			return;
		}
		CharacterStats component = GetComponent<CharacterStats>();
		int num = m_currentItems.SelectedWeaponSet;
		bool flag = m_currentItems.GetWeaponSet(m_currentItems.SelectedWeaponSet).Empty();
		bool flag3;
		do
		{
			num = (num + 1) % component.MaxWeaponSets;
			bool flag2 = m_currentItems.GetWeaponSet(num).Empty();
			flag3 = IsWeaponSetValid(num) || (flag2 && !flag);
			if (flag2)
			{
				flag = true;
			}
		}
		while (!flag3 && num != m_currentItems.SelectedWeaponSet);
		SelectWeaponSet(num, enforceRecoveryPenalty: true);
	}

	public void SelectWeaponSet(int index, bool enforceRecoveryPenalty)
	{
		if (index >= 0 && index != m_currentItems.SelectedWeaponSet && CanChangeWeaponSets())
		{
			ClearSummonEffectInSlot(Equippable.EquipmentSlot.PrimaryWeapon);
			ClearSummonEffectInSlot(Equippable.EquipmentSlot.SecondaryWeapon);
			Equippable primaryWeapon = CurrentItems.PrimaryWeapon;
			Equippable secondaryWeapon = CurrentItems.SecondaryWeapon;
			CharacterStats component = GetComponent<CharacterStats>();
			if (!component || !component.IsEquipmentLocked)
			{
				m_currentItems.SelectWeaponSet(base.gameObject, index);
			}
			m_primaryWeapon = null;
			m_secondaryWeapon = null;
			GlobalAudioPlayer.SPlay(m_currentItems.GetSelectedWeaponSet());
			if (this.OnEquipmentChanged != null)
			{
				this.OnEquipmentChanged(Equippable.EquipmentSlot.PrimaryWeapon, primaryWeapon, CurrentItems.PrimaryWeapon, m_isSwappingSummonedWeapon, enforceRecoveryPenalty);
				this.OnEquipmentChanged(Equippable.EquipmentSlot.SecondaryWeapon, secondaryWeapon, CurrentItems.SecondaryWeapon, m_isSwappingSummonedWeapon, enforceRecoveryPenalty);
			}
			PartyMemberAI component2 = GetComponent<PartyMemberAI>();
			if (component2 != null && component2.gameObject.activeInHierarchy)
			{
				component2.UpdateStateAfterWeaponSetChange(becauseSummoningWeapon: false);
			}
		}
	}

	public Equippable Equip(Equippable item)
	{
		return Equip(item, m_currentItems.GetDesiredSlot(item), enforceRecoveryPenalty: true);
	}

	public void ReloadWeapons()
	{
		EquipmentSet currentItems = CurrentItems;
		if (currentItems.PrimaryWeapon != null)
		{
			Equippable equippable = GameResources.Instantiate<Equippable>(currentItems.PrimaryWeapon);
			equippable.Prefab = currentItems.PrimaryWeapon;
			UnEquip(currentItems.PrimaryWeapon);
			GameUtilities.RecursiveSetLayer(equippable.gameObject, base.gameObject.layer);
			currentItems.AlternateWeaponSets[0].PrimaryWeapon = equippable;
			Equip(equippable, Equippable.EquipmentSlot.PrimaryWeapon, enforceRecoveryPenalty: false);
		}
		if (currentItems.SecondaryWeapon != null)
		{
			Equippable equippable2 = GameResources.Instantiate<Equippable>(currentItems.SecondaryWeapon);
			equippable2.Prefab = currentItems.SecondaryWeapon;
			UnEquip(currentItems.SecondaryWeapon);
			GameUtilities.RecursiveSetLayer(equippable2.gameObject, base.gameObject.layer);
			currentItems.AlternateWeaponSets[0].SecondaryWeapon = equippable2;
			Equip(equippable2, Equippable.EquipmentSlot.SecondaryWeapon, enforceRecoveryPenalty: false);
		}
		SelectWeaponSet(currentItems.SelectedWeaponSet, enforceRecoveryPenalty: false);
	}

	public Equippable Equip(Equippable item, Equippable.EquipmentSlot desiredSlot, bool enforceRecoveryPenalty)
	{
		if (item == null)
		{
			return null;
		}
		switch (desiredSlot)
		{
		case Equippable.EquipmentSlot.PrimaryWeapon2:
		case Equippable.EquipmentSlot.SecondaryWeapon2:
			Debug.LogError("Use EquipWeapon to equip weapons.");
			return null;
		case Equippable.EquipmentSlot.PrimaryWeapon:
		case Equippable.EquipmentSlot.SecondaryWeapon:
			return EquipWeapon(item, desiredSlot, m_currentItems.SelectedWeaponSet);
		default:
		{
			if (IsSlotLocked(desiredSlot))
			{
				return null;
			}
			if (desiredSlot == Equippable.EquipmentSlot.Grimoire && !m_initalizingSet)
			{
				CharacterStats component = GetComponent<CharacterStats>();
				if ((bool)component)
				{
					NPCAppearance component2 = GetComponent<NPCAppearance>();
					if (component2 == null || !component2.IsCreatingAppearance)
					{
						component.CoolDownGrimoire();
					}
				}
			}
			Equippable equippable = m_currentItems.GetItemInSlot(desiredSlot);
			if ((bool)equippable)
			{
				equippable = UnEquip(equippable);
			}
			m_currentItems.EquipItemToSlot(base.gameObject, item, desiredSlot);
			if (this.OnEquipmentChanged != null)
			{
				this.OnEquipmentChanged(desiredSlot, equippable, item, m_isSwappingSummonedWeapon, enforceRecoveryPenalty);
			}
			return equippable;
		}
		}
	}

	public Equippable EquipWeapon(Equippable item, Equippable.EquipmentSlot desiredSlot, int weaponSetNumber)
	{
		if (IsSlotLocked(desiredSlot) && (!Equippable.IsWeapon(desiredSlot) || weaponSetNumber == m_currentItems.SelectedWeaponSet))
		{
			return null;
		}
		if (!m_isSwappingSummonedWeapon)
		{
			if (desiredSlot == Equippable.EquipmentSlot.PrimaryWeapon)
			{
				ClearSummonEffectInSlot(Equippable.EquipmentSlot.PrimaryWeapon);
			}
			if (desiredSlot == Equippable.EquipmentSlot.SecondaryWeapon || item.BothPrimaryAndSecondarySlot)
			{
				ClearSummonEffectInSlot(Equippable.EquipmentSlot.SecondaryWeapon);
			}
		}
		Equippable equippable = m_currentItems.GetWeaponInSlot(desiredSlot, weaponSetNumber);
		if ((bool)equippable)
		{
			equippable = UnEquipWeapon(equippable, desiredSlot, weaponSetNumber);
		}
		if (item != null)
		{
			m_currentItems.EquipWeaponToSlot(base.gameObject, item, desiredSlot, weaponSetNumber);
		}
		if (this.OnEquipmentChanged != null)
		{
			this.OnEquipmentChanged(desiredSlot, equippable, item, m_isSwappingSummonedWeapon, enforceRecoveryPenalty: true);
		}
		return equippable;
	}

	public Equippable UnEquip(Equippable.EquipmentSlot slot)
	{
		Equippable itemInSlot = m_currentItems.GetItemInSlot(slot);
		return UnEquip(itemInSlot, slot);
	}

	public Equippable UnEquip(Equippable item)
	{
		return UnEquip(item, m_currentItems.GetSlot(item));
	}

	public Equippable UnEquip(Equippable item, Equippable.EquipmentSlot slot)
	{
		if (item == null)
		{
			return null;
		}
		switch (slot)
		{
		case Equippable.EquipmentSlot.PrimaryWeapon2:
		case Equippable.EquipmentSlot.SecondaryWeapon2:
			Debug.LogError("Use UnEquipWeapon to unequip weapons.");
			return null;
		case Equippable.EquipmentSlot.PrimaryWeapon:
		case Equippable.EquipmentSlot.SecondaryWeapon:
			return UnEquipWeapon(item, slot, m_currentItems.SelectedWeaponSet);
		default:
		{
			if (IsSlotLocked(slot))
			{
				return null;
			}
			Equippable equippable = item.UnEquip(base.gameObject, slot);
			m_currentItems.EquipItemToSlot(base.gameObject, null, slot);
			if (this.OnEquipmentChanged != null)
			{
				this.OnEquipmentChanged(slot, equippable, null, m_isSwappingSummonedWeapon, enforceRecoveryPenalty: false);
			}
			return equippable;
		}
		}
	}

	public Equippable UnEquipWeapon(Equippable item)
	{
		int weaponSetIndexForItem = m_currentItems.GetWeaponSetIndexForItem(item);
		if (weaponSetIndexForItem >= 0)
		{
			return UnEquipWeapon(item, m_currentItems.GetSlot(item), weaponSetIndexForItem);
		}
		return null;
	}

	public Equippable UnEquipWeapon(Equippable item, int weaponSet)
	{
		return UnEquipWeapon(item, m_currentItems.GetSlot(item), weaponSet);
	}

	public Equippable UnEquipWeapon(Equippable item, Equippable.EquipmentSlot slot, int weaponSet)
	{
		if (IsSlotLocked(slot) && (!Equippable.IsWeapon(slot) || weaponSet == m_currentItems.SelectedWeaponSet))
		{
			return null;
		}
		m_primaryWeapon = null;
		m_secondaryWeapon = null;
		if (item.IsSummoned && !m_isSwappingSummonedWeapon)
		{
			ClearSummonEffectInSlot(slot);
			item = m_currentItems.GetItemInSlot(slot);
		}
		Equippable equippable = item.UnEquip(base.gameObject, slot);
		m_currentItems.EquipWeaponToSlot(base.gameObject, null, slot, weaponSet);
		if (this.OnEquipmentChanged != null)
		{
			this.OnEquipmentChanged(slot, equippable, null, m_isSwappingSummonedWeapon, enforceRecoveryPenalty: false);
		}
		return equippable;
	}

	public float CalculateDT(DamagePacket.DamageType dmgType, float bonusDT, GameObject wearer)
	{
		if (CurrentItems != null)
		{
			return CurrentItems.CalculateDT(dmgType, bonusDT, wearer);
		}
		return 0f;
	}

	public float CalculateDR(DamagePacket.DamageType dmgType)
	{
		if (CurrentItems != null)
		{
			return CurrentItems.CalculateDR(dmgType);
		}
		return 0f;
	}

	public float CalculatePrefabDT(DamagePacket.DamageType dmgType, float bonusDT, GameObject wearer)
	{
		if (DefaultEquippedItems != null)
		{
			return DefaultEquippedItems.CalculateDT(dmgType, bonusDT, wearer);
		}
		return 0f;
	}

	public float CalculatePrefabDR(DamagePacket.DamageType dmgType)
	{
		if (DefaultEquippedItems != null)
		{
			return DefaultEquippedItems.CalculateDR(dmgType);
		}
		return 0f;
	}

	public bool IsEquipped(string itemName)
	{
		for (Equippable.EquipmentSlot equipmentSlot = Equippable.EquipmentSlot.Head; equipmentSlot < Equippable.EquipmentSlot.Count; equipmentSlot++)
		{
			Equippable itemInSlot = CurrentItems.GetItemInSlot(equipmentSlot);
			if (itemInSlot != null && itemInSlot.Prefab.name == itemName)
			{
				return true;
			}
		}
		return false;
	}

	public void PushSummonedWeapon(Equippable summonedWeapon, Equippable.EquipmentSlot slot, StatusEffect effect)
	{
		m_isSwappingSummonedWeapon = true;
		Equippable equippable = UnityEngine.Object.Instantiate(summonedWeapon);
		equippable.IsSummoned = true;
		equippable.SummoningEffect = effect;
		equippable.Prefab = summonedWeapon;
		equippable.Location = Item.ItemLocation.Stored;
		equippable.transform.parent = base.transform;
		equippable.transform.localPosition = Vector3.zero;
		equippable.transform.localRotation = Quaternion.identity;
		GameState.PersistAcrossSceneLoadsTracked(equippable);
		Persistence component = equippable.GetComponent<Persistence>();
		if ((bool)component)
		{
			component.UnloadsBetweenLevels = false;
		}
		if (slot == Equippable.EquipmentSlot.SecondaryWeapon)
		{
			Equippable equippable2 = EquipWeapon(equippable, slot, m_currentItems.SelectedWeaponSet);
			if ((bool)equippable2)
			{
				if (equippable2.IsSummoned)
				{
					GameUtilities.Destroy(equippable2.gameObject);
				}
				else
				{
					m_currentItems.PushedSecondaryWeapon = UnEquipWeapon(equippable2, m_currentItems.SelectedWeaponSet);
				}
			}
			LockSlot(Equippable.EquipmentSlot.SecondaryWeapon);
		}
		if (slot == Equippable.EquipmentSlot.PrimaryWeapon)
		{
			if (equippable.BothPrimaryAndSecondarySlot)
			{
				Equippable weaponInSlot = m_currentItems.GetWeaponInSlot(Equippable.EquipmentSlot.SecondaryWeapon, m_currentItems.SelectedWeaponSet);
				if (weaponInSlot != null)
				{
					weaponInSlot = UnEquipWeapon(weaponInSlot, Equippable.EquipmentSlot.SecondaryWeapon, m_currentItems.SelectedWeaponSet);
					if (weaponInSlot != null)
					{
						if (weaponInSlot.IsSummoned)
						{
							GameUtilities.Destroy(weaponInSlot.gameObject);
						}
						else
						{
							m_currentItems.PushedSecondaryWeapon = UnEquipWeapon(weaponInSlot, m_currentItems.SelectedWeaponSet);
						}
					}
				}
				LockSlot(Equippable.EquipmentSlot.SecondaryWeapon);
			}
			Equippable equippable3 = EquipWeapon(equippable, Equippable.EquipmentSlot.PrimaryWeapon, m_currentItems.SelectedWeaponSet);
			if ((bool)equippable3)
			{
				if (equippable3.IsSummoned)
				{
					GameUtilities.Destroy(equippable3.gameObject);
				}
				else
				{
					m_currentItems.PushedPrimaryWeapon = equippable3;
				}
			}
			LockSlot(Equippable.EquipmentSlot.PrimaryWeapon);
		}
		m_isSwappingSummonedWeapon = false;
	}

	public bool PopSummonedWeapon(Equippable.EquipmentSlot slot)
	{
		m_isSwappingSummonedWeapon = true;
		bool result = false;
		Equippable equippable = null;
		Equippable itemInSlot = m_currentItems.GetItemInSlot(slot);
		if ((bool)itemInSlot && itemInSlot.IsSummoned)
		{
			UnlockSlot(slot);
			Equippable item;
			if (slot == Equippable.EquipmentSlot.PrimaryWeapon)
			{
				item = m_currentItems.PushedPrimaryWeapon;
				m_currentItems.PushedPrimaryWeapon = null;
			}
			else
			{
				item = m_currentItems.PushedSecondaryWeapon;
				m_currentItems.PushedSecondaryWeapon = null;
			}
			equippable = EquipWeapon(item, slot, m_currentItems.SelectedWeaponSet);
			result = true;
		}
		if ((bool)equippable && equippable.BothPrimaryAndSecondarySlot)
		{
			UnlockSlot(Equippable.EquipmentSlot.SecondaryWeapon);
			Equippable equippable2 = EquipWeapon(m_currentItems.PushedSecondaryWeapon, Equippable.EquipmentSlot.SecondaryWeapon, m_currentItems.SelectedWeaponSet);
			m_currentItems.PushedSecondaryWeapon = null;
			if ((bool)equippable2)
			{
				if (equippable2.IsSummoned)
				{
					GameUtilities.Destroy(equippable2.gameObject);
				}
				else
				{
					UIDebug.Instance.LogOnScreenWarning("Tried to pop secondary weapon '" + equippable2.name + "' but it wasn't summoned.", UIDebug.Department.Programming, 10f);
				}
				result = true;
			}
		}
		if ((bool)equippable)
		{
			GameUtilities.Destroy(equippable.gameObject);
		}
		m_isSwappingSummonedWeapon = false;
		return result;
	}

	public void ClearSummonEffectInSlot(Equippable.EquipmentSlot slot)
	{
		CharacterStats component = base.gameObject.GetComponent<CharacterStats>();
		if (component != null)
		{
			component.ClearEffects((slot == Equippable.EquipmentSlot.PrimaryWeapon) ? StatusEffect.ModifiedStat.SummonWeapon : StatusEffect.ModifiedStat.SummonSecondaryWeapon);
			PopSummonedWeapon(slot);
		}
	}

	public void HandleAnimShowSlot(object obj, EventArgs args)
	{
		if (FindSlotFromString(obj.ToString(), out var slot))
		{
			Equippable itemInSlot = m_currentItems.GetItemInSlot(slot);
			if (itemInSlot != null)
			{
				itemInSlot.gameObject.SetActive(value: true);
			}
		}
	}

	public void HandleAnimHideSlot(object obj, EventArgs args)
	{
		if (FindSlotFromString(obj.ToString(), out var slot))
		{
			Equippable itemInSlot = m_currentItems.GetItemInSlot(slot);
			if (itemInSlot != null)
			{
				itemInSlot.gameObject.SetActive(value: false);
			}
		}
	}

	public void HandleAnimMoveToHand(object obj, EventArgs args)
	{
		if (FindSlotFromString(obj.ToString(), out var slot))
		{
			Equippable itemInSlot = m_currentItems.GetItemInSlot(slot);
			if (itemInSlot != null)
			{
				itemInSlot.AttachToSlot(base.gameObject, Equippable.EquipmentSlot.PrimaryWeapon);
			}
		}
	}

	public void HandleAnimMoveFromHand(object obj, EventArgs args)
	{
		if (FindSlotFromString(obj.ToString(), out var slot))
		{
			Equippable itemInSlot = m_currentItems.GetItemInSlot(slot);
			if (itemInSlot != null)
			{
				itemInSlot.AttachToSlot(base.gameObject, slot);
			}
		}
	}

	private bool FindSlotFromString(string slotName, out Equippable.EquipmentSlot slot)
	{
		slot = Equippable.EquipmentSlot.Head;
		for (Equippable.EquipmentSlot equipmentSlot = Equippable.EquipmentSlot.Head; equipmentSlot < Equippable.EquipmentSlot.Count; equipmentSlot++)
		{
			if (string.Compare(slotName, equipmentSlot.ToString(), ignoreCase: true) == 0)
			{
				slot = equipmentSlot;
				return true;
			}
		}
		return false;
	}
}
