using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Polenter.Serialization;
using UnityEngine;

public class Equippable : Item
{
	public enum EquipmentSlot
	{
		Head,
		Neck,
		Armor,
		RightRing,
		LeftRing,
		Hands,
		Cape_DEPRECATED,
		Feet,
		Waist,
		Grimoire,
		Pet,
		PrimaryWeapon,
		SecondaryWeapon,
		Count,
		PrimaryWeapon2,
		SecondaryWeapon2,
		None
	}

	public enum CantEquipReason
	{
		None,
		Other,
		EquipmentLocked,
		ClassMismatch,
		SoulboundToOther
	}

	private struct SavedBoneTransform
	{
		public Vector3 Position;

		public Quaternion Rotation;

		public Vector3 Scale;
	}

	public enum DurabilityStateType
	{
		Normal,
		Worn,
		Damaged
	}

	private const string mPrimaryWeaponBone = "primaryWeapon";

	private const string mSecondaryWeaponBone = "secondaryWeapon";

	private const string mWeaponAttachmentBone = "h_weapon";

	private const string mSecondaryWeaponAttachmentBone = "a_weapon";

	private const string mPrimaryScabbardBone = "primaryScabbard";

	private const string mSecondaryScabbardBone = "secondaryScabbard";

	private const string mBackScabbardBone = "backScabbard";

	private const string mShieldBone = "LeftForeArm_Att";

	private const string mShieldAttachmentBone = "l_foreArmWeapon";

	public bool HeadSlot;

	public bool NeckSlot;

	public bool ArmorSlot;

	public bool RingRightHandSlot;

	public bool RingLeftHandSlot;

	public bool HandSlot;

	public bool FeetSlot;

	public bool WaistSlot;

	public bool GrimoireSlot;

	public bool PrimaryWeaponSlot;

	public bool SecondaryWeaponSlot;

	public bool BothPrimaryAndSecondarySlot;

	public bool PetSlot;

	[NonSerialized]
	[ExcludeFromSerialization]
	[HideInInspector]
	public Ref<bool>[] Slots = new Ref<bool>[13];

	public AppearancePiece Appearance;

	public bool StatusEffectsExtraObjectIsSummoner;

	public StatusEffectParams[] StatusEffects;

	public ItemMod[] ItemMods;

	public GameObject OnEquipVisualEffect;

	[Tooltip("If empty, all classes can equip this item. Otherwise, only the listed classes can equip it.")]
	public CharacterStats.Class[] RestrictedToClass;

	[Tooltip("If set, item can't be unequipped when worn.")]
	public bool CantUnequip;

	private float MaxDurability = 100f;

	private float m_durability;

	private List<ItemModComponent> m_itemModComponents = new List<ItemModComponent>();

	private List<GameObject> m_activeFX = new List<GameObject>();

	private bool m_listening_for_charged_usage;

	private GameObject m_owner;

	private AlphaControl m_ownerAlphaControl;

	private EquipmentSlot m_equipped_slot = EquipmentSlot.None;

	private List<Transform> m_originalParentTransforms;

	private List<Transform> m_originalChildTransforms;

	private List<SavedBoneTransform> m_originalBoneTransforms;

	private bool ActiveWhenWieldedDelay;

	[HideInInspector]
	public Equipment m_ownerEquipment;

	public GameObject[] ActiveWhenWielded;

	private bool m_tryEquipAgain;

	private List<ItemMod> m_modsSerialized = new List<ItemMod>();

	public List<Guid> AbilityModGuidsSerialized = new List<Guid>();

	public static bool UiDetailedContent;

	protected readonly AttackBase.FormattableTarget TARGET_USER = new AttackBase.FormattableTarget(1610);

	public List<ItemModComponent> AttachedItemMods => m_itemModComponents;

	public GameObject EquippedOwner => m_owner;

	public bool IsAnimating { get; set; }

	public bool IsSummoned { get; set; }

	public StatusEffect SummoningEffect { get; set; }

	public EquipmentSlot EquippedSlot => m_equipped_slot;

	[Persistent]
	public bool HasAddedPrefabMods { get; set; }

	[Persistent]
	public List<ItemMod> Mods
	{
		get
		{
			m_modsSerialized.Clear();
			foreach (ItemModComponent itemModComponent in m_itemModComponents)
			{
				if (itemModComponent.Mod != null)
				{
					m_modsSerialized.Add(itemModComponent.Mod);
				}
			}
			return m_modsSerialized;
		}
		set
		{
			if (value == m_modsSerialized)
			{
				return;
			}
			foreach (ItemMod item in value)
			{
				if (item != null)
				{
					m_modsSerialized.Add(item);
				}
			}
		}
	}

	[Persistent]
	public List<Guid> AbilityModGuids
	{
		get
		{
			AbilityModGuidsSerialized.Clear();
			foreach (ItemModComponent itemModComponent in m_itemModComponents)
			{
				if (itemModComponent.Ability != null)
				{
					InstanceID component = itemModComponent.Ability.GetComponent<InstanceID>();
					if ((bool)component && component.Guid != Guid.Empty)
					{
						AbilityModGuidsSerialized.Add(component.Guid);
					}
				}
			}
			return AbilityModGuidsSerialized;
		}
		set
		{
			AbilityModGuidsSerialized = value;
		}
	}

	public CurrencyValue RepairCost => new CurrencyValue
	{
		v = (1f - m_durability / MaxDurability) * Value.v
	};

	public DurabilityStateType DurabilityState => DurabilityStateType.Normal;

	public event Action<Equippable> ItemModsChanged;

	public static bool IsWeapon(EquipmentSlot slot)
	{
		if (!IsPrimaryWeapon(slot))
		{
			return IsSecondaryWeapon(slot);
		}
		return true;
	}

	public static bool IsPrimaryWeapon(EquipmentSlot slot)
	{
		if (slot != EquipmentSlot.PrimaryWeapon)
		{
			return slot == EquipmentSlot.PrimaryWeapon2;
		}
		return true;
	}

	public static bool IsSecondaryWeapon(EquipmentSlot slot)
	{
		if (slot != EquipmentSlot.SecondaryWeapon)
		{
			return slot == EquipmentSlot.SecondaryWeapon2;
		}
		return true;
	}

	public override float GetValue()
	{
		if (!m_Initted)
		{
			Init();
		}
		float num = 0f;
		if (m_itemModComponents != null)
		{
			for (int i = 0; i < m_itemModComponents.Count; i++)
			{
				ItemModComponent itemModComponent = m_itemModComponents[i];
				if (itemModComponent != null && itemModComponent.Mod != null)
				{
					num += (float)(itemModComponent.Mod.Cost * EconomyManager.Instance.ItemModCostMultiplier * ((!BothPrimaryAndSecondarySlot) ? 1 : 2));
				}
			}
		}
		return base.GetValue() + num;
	}

	public Equippable()
	{
		Slots[0] = new Ref<bool>(() => HeadSlot, delegate(bool x)
		{
			HeadSlot = x;
		});
		Slots[1] = new Ref<bool>(() => NeckSlot, delegate(bool x)
		{
			NeckSlot = x;
		});
		Slots[2] = new Ref<bool>(() => ArmorSlot, delegate(bool x)
		{
			ArmorSlot = x;
		});
		Slots[3] = new Ref<bool>(() => RingRightHandSlot, delegate(bool x)
		{
			RingRightHandSlot = x;
		});
		Slots[4] = new Ref<bool>(() => RingLeftHandSlot, delegate(bool x)
		{
			RingLeftHandSlot = x;
		});
		Slots[5] = new Ref<bool>(() => HandSlot, delegate(bool x)
		{
			HandSlot = x;
		});
		Slots[6] = new Ref<bool>(() => false, delegate
		{
		});
		Slots[7] = new Ref<bool>(() => FeetSlot, delegate(bool x)
		{
			FeetSlot = x;
		});
		Slots[8] = new Ref<bool>(() => WaistSlot, delegate(bool x)
		{
			WaistSlot = x;
		});
		Slots[9] = new Ref<bool>(() => GrimoireSlot, delegate(bool x)
		{
			GrimoireSlot = x;
		});
		Slots[11] = new Ref<bool>(() => PrimaryWeaponSlot, delegate(bool x)
		{
			PrimaryWeaponSlot = x;
		});
		Slots[12] = new Ref<bool>(() => SecondaryWeaponSlot, delegate(bool x)
		{
			SecondaryWeaponSlot = x;
		});
		Slots[10] = new Ref<bool>(() => PetSlot, delegate(bool x)
		{
			PetSlot = x;
		});
	}

	public override void Awake()
	{
		base.Awake();
		if (MaxDurability <= 1f)
		{
			MaxDurability = 1f;
		}
		m_durability = MaxDurability;
	}

	public override void Init()
	{
		if (!m_Initted && (bool)this)
		{
			m_Initted = true;
			EquipmentSoulbind component = GetComponent<EquipmentSoulbind>();
			if ((bool)component && (bool)component.GetOverrideAppearance())
			{
				RebuildAppearance();
			}
			RecacheTransforms();
			AttachMods();
		}
	}

	public void RecacheTransforms()
	{
		if (m_originalParentTransforms == null)
		{
			m_originalChildTransforms = new List<Transform>();
			m_originalParentTransforms = new List<Transform>();
			m_originalBoneTransforms = new List<SavedBoneTransform>();
		}
		else
		{
			m_originalChildTransforms.Clear();
			m_originalParentTransforms.Clear();
			m_originalBoneTransforms.Clear();
		}
		StoreOriginalTransforms(base.gameObject.transform);
	}

	public void RebuildAppearance()
	{
		GameObject gameObject = base.Prefab.gameObject;
		EquipmentSoulbind component = GetComponent<EquipmentSoulbind>();
		if ((bool)component)
		{
			GameObject overrideAppearance = component.GetOverrideAppearance();
			if ((bool)overrideAppearance)
			{
				gameObject = overrideAppearance;
			}
		}
		if (!gameObject || gameObject == this)
		{
			Debug.LogError("Failed to rebuild equipment appearance: no prefab found (" + base.name + ")");
			return;
		}
		Transform transform = base.transform;
		foreach (Transform item in transform)
		{
			GameUtilities.Destroy(item.gameObject);
		}
		GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject, transform.position, transform.rotation);
		foreach (Transform item2 in gameObject2.transform)
		{
			item2.parent = transform;
		}
		GameUtilities.Destroy(gameObject2);
		m_renderers = null;
	}

	public bool CanUnequip()
	{
		if (CantUnequip)
		{
			return false;
		}
		for (int i = 0; i < AttachedItemMods.Count; i++)
		{
			if (AttachedItemMods[i].Mod.CursesItem)
			{
				return false;
			}
		}
		return true;
	}

	private void AttachMods()
	{
		if ((HasAddedPrefabMods && (bool)GetComponent<EquipmentSoulbind>()) || ItemMods == null)
		{
			return;
		}
		ItemMod[] itemMods = ItemMods;
		foreach (ItemMod itemMod in itemMods)
		{
			bool flag = false;
			if (itemMod != null && itemMod.IsQualityMod)
			{
				foreach (ItemModComponent attachedItemMod in AttachedItemMods)
				{
					if (attachedItemMod != null && attachedItemMod.Mod != null && attachedItemMod.Mod.IsQualityMod)
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				AttachItemMod(itemMod);
			}
		}
		HasAddedPrefabMods = true;
		CheckForListeningForChargedUsage();
	}

	private void CheckForListeningForChargedUsage()
	{
		bool flag = true;
		foreach (ItemModComponent itemModComponent in m_itemModComponents)
		{
			if (!itemModComponent.Charged)
			{
				flag = false;
			}
		}
		if (m_itemModComponents.Count > 0 && flag)
		{
			StartListeningForChargedUsage();
		}
	}

	public int GetNumberOfModsOfEnchantCategory(ItemMod.EnchantCategory category, int withCostGreaterThan)
	{
		int num = 0;
		ItemModComponent itemModComponent = null;
		for (int i = 0; i < m_itemModComponents.Count; i++)
		{
			itemModComponent = m_itemModComponents[i];
			if (!(itemModComponent == null) && !(itemModComponent.Mod == null) && itemModComponent.Mod.ModEnchantCategory == category && itemModComponent.Mod.Cost >= withCostGreaterThan)
			{
				num++;
			}
		}
		return num;
	}

	public int TotalItemModValue()
	{
		int num = 0;
		if (base.IsPrefab)
		{
			for (int i = 0; i < ItemMods.Length; i++)
			{
				if ((bool)ItemMods[i])
				{
					num += ItemMods[i].Cost;
				}
			}
		}
		else
		{
			if (!m_Initted)
			{
				Init();
			}
			if (m_itemModComponents == null || m_itemModComponents.Count == 0)
			{
				return 0;
			}
			ItemModComponent itemModComponent = null;
			for (int j = 0; j < m_itemModComponents.Count; j++)
			{
				itemModComponent = m_itemModComponents[j];
				if (!(itemModComponent == null) && !(itemModComponent.Mod == null))
				{
					num += itemModComponent.Mod.Cost;
				}
			}
		}
		return num;
	}

	public bool NeedsItemModIndicator()
	{
		return TotalItemModValue() > 0;
	}

	public void AttachItemMod(ItemMod mod)
	{
		if (PE_Paperdoll.IsObjectPaperdoll(base.gameObject) || !(mod != null))
		{
			return;
		}
		foreach (ItemModComponent itemModComponent2 in m_itemModComponents)
		{
			if (itemModComponent2.Mod.Equals(mod))
			{
				return;
			}
		}
		if (mod.IsQualityMod)
		{
			for (int num = m_itemModComponents.Count - 1; num >= 0; num--)
			{
				if (m_itemModComponents[num].Mod.IsQualityMod)
				{
					DestroyItemMod(m_itemModComponents[num]);
				}
			}
		}
		ItemModComponent itemModComponent = ItemModComponent.Create(this, mod);
		if (m_listening_for_charged_usage && !itemModComponent.Charged)
		{
			StopListeningForChargedUsage();
		}
		m_itemModComponents.Add(itemModComponent);
		if (this.ItemModsChanged != null)
		{
			this.ItemModsChanged(this);
		}
		if ((bool)EquippedOwner)
		{
			itemModComponent.ApplyEquipEffects(EquippedOwner, EquippedSlot, EquipmentSlotToAbilityType(EquippedSlot), this);
		}
	}

	public bool DestroyFirstMod(ItemMod mod)
	{
		for (int i = 0; i < m_itemModComponents.Count; i++)
		{
			if (m_itemModComponents[i].IsInstanceOf(mod))
			{
				DestroyItemMod(m_itemModComponents[i]);
				return true;
			}
		}
		return false;
	}

	public void DestroyItemMod(ItemModComponent mod)
	{
		if (mod != null && m_itemModComponents.Remove(mod))
		{
			if (m_listening_for_charged_usage)
			{
				StopListeningForChargedUsage();
			}
			if (this.ItemModsChanged != null)
			{
				this.ItemModsChanged(this);
			}
			CheckForListeningForChargedUsage();
			GameUtilities.Destroy(mod, 1f);
		}
	}

	public void ResetItemMods()
	{
		if (m_itemModComponents != null)
		{
			for (int num = m_itemModComponents.Count - 1; num >= 0; num--)
			{
				DestroyItemMod(m_itemModComponents[num]);
			}
		}
		if (ItemMods != null)
		{
			ItemMod[] itemMods = ItemMods;
			foreach (ItemMod mod in itemMods)
			{
				AttachItemMod(mod);
			}
		}
	}

	public override void OnDestroy()
	{
		if (m_tryEquipAgain && m_owner != null && base.gameObject != null)
		{
			Debug.LogWarning(m_owner.name + " destroying equipped item on character " + base.gameObject.name, base.gameObject);
		}
		if (m_listening_for_charged_usage)
		{
			StopListeningForChargedUsage();
		}
		base.OnDestroy();
		ComponentUtils.NullOutObjectReferences(this);
	}

	public bool CanUseSlot(EquipmentSlot slot)
	{
		switch (slot)
		{
		case EquipmentSlot.PrimaryWeapon:
		case EquipmentSlot.PrimaryWeapon2:
			return PrimaryWeaponSlot;
		case EquipmentSlot.SecondaryWeapon:
		case EquipmentSlot.SecondaryWeapon2:
			if (SecondaryWeaponSlot)
			{
				return !BothPrimaryAndSecondarySlot;
			}
			return false;
		default:
			if ((int)slot < Slots.Length)
			{
				return Slots[(int)slot].Val;
			}
			return false;
		}
	}

	public EquipmentSlot GetPreferredSlot()
	{
		for (int i = 0; i < Slots.Length; i++)
		{
			if (Slots[i].Val)
			{
				return (EquipmentSlot)i;
			}
		}
		return EquipmentSlot.None;
	}

	public bool CanEquip(GameObject character)
	{
		return WhyCantEquip(character) == CantEquipReason.None;
	}

	public virtual CantEquipReason WhyCantEquip(GameObject character)
	{
		if (!character)
		{
			return CantEquipReason.Other;
		}
		EquipmentSoulbind component = GetComponent<EquipmentSoulbind>();
		InstanceID component2 = character.GetComponent<InstanceID>();
		if ((bool)component && component.IsBound && (!component2 || component2.Guid != component.BoundGuid))
		{
			return CantEquipReason.SoulboundToOther;
		}
		CharacterStats component3 = character.GetComponent<CharacterStats>();
		if (component3 != null)
		{
			if (component3.IsEquipmentLocked)
			{
				return CantEquipReason.EquipmentLocked;
			}
			if (RestrictedToClass != null)
			{
				bool flag = true;
				CharacterStats.Class[] restrictedToClass = RestrictedToClass;
				foreach (CharacterStats.Class @class in restrictedToClass)
				{
					if (component3.CharacterClass == @class)
					{
						flag = true;
						break;
					}
					if (@class != 0)
					{
						flag = false;
					}
				}
				if (!flag)
				{
					return CantEquipReason.ClassMismatch;
				}
			}
		}
		return CantEquipReason.None;
	}

	private void Update()
	{
		Transform transform = base.transform;
		if ((bool)m_owner && !m_ownerAlphaControl)
		{
			m_ownerAlphaControl = m_owner.GetComponent<AlphaControl>();
		}
		if (EquippedOwner == null || transform.parent == null || transform.parent.transform == EquippedOwner.transform)
		{
			if (base.Renders)
			{
				base.Renders = false;
			}
		}
		else if (m_equipped_slot == EquipmentSlot.Grimoire && !IsAnimating && m_ownerEquipment != null && !m_ownerEquipment.ShouldGrimoireBeDisplayed())
		{
			if (base.Renders)
			{
				base.Renders = false;
			}
		}
		else if (!m_ownerAlphaControl || m_ownerAlphaControl.Alpha > 0f)
		{
			if (!base.Renders)
			{
				base.Renders = true;
				if ((bool)m_ownerAlphaControl)
				{
					m_ownerAlphaControl.Refresh();
				}
			}
		}
		else if (base.Renders)
		{
			base.Renders = false;
		}
		if (base.gameObject.activeInHierarchy && ActiveWhenWieldedDelay && ActiveWhenWielded != null)
		{
			for (int i = 0; i < ActiveWhenWielded.Length; i++)
			{
				if (ActiveWhenWielded[i] != null)
				{
					ActiveWhenWielded[i].SetActive(value: false);
				}
			}
			ActiveWhenWieldedDelay = false;
		}
		if (transform.parent == null && m_owner != null && m_equipped_slot != EquipmentSlot.None)
		{
			m_tryEquipAgain = true;
		}
		if (m_tryEquipAgain && m_owner != null)
		{
			Debug.LogWarning(m_owner.name + " attempting to reequip weapon " + base.gameObject.name, base.gameObject);
			if (!AttachToSlot(m_owner, m_equipped_slot))
			{
				m_tryEquipAgain = true;
			}
			else
			{
				CreateBoneMapping();
			}
		}
	}

	public virtual void Equip(GameObject character, EquipmentSlot slot)
	{
		if (!CanEquip(character))
		{
			return;
		}
		if (!m_Initted)
		{
			Init();
		}
		m_owner = character;
		m_ownerAlphaControl = (m_owner ? m_owner.GetComponent<AlphaControl>() : null);
		m_ownerEquipment = m_owner.GetComponent<Equipment>();
		m_equipped_slot = slot;
		Health component = m_owner.GetComponent<Health>();
		if ((bool)component)
		{
			component.OnDeath += h_OnDeath;
		}
		if (slot != EquipmentSlot.PrimaryWeapon2 && slot != EquipmentSlot.SecondaryWeapon2)
		{
			SendMessage("OnEquip", character, SendMessageOptions.DontRequireReceiver);
		}
		CharacterStats component2 = character.GetComponent<CharacterStats>();
		if (component2 != null)
		{
			GenericAbility.AbilityType abType = EquipmentSlotToAbilityType(slot);
			if (!GameState.LoadedGame)
			{
				StatusEffectParams[] statusEffects = StatusEffects;
				foreach (StatusEffectParams param in statusEffects)
				{
					StatusEffect statusEffect = StatusEffect.Create(character, this, param, abType, null, deleteOnClear: false);
					statusEffect.Slot = slot;
					if (StatusEffectsExtraObjectIsSummoner && SummoningEffect != null)
					{
						statusEffect.ExtraObject = SummoningEffect.Owner;
					}
					component2.ApplyStatusEffect(statusEffect);
				}
			}
			ApplyItemModEquipEffects(character, slot, abType);
		}
		base.Location = ItemLocation.Equipped;
		if (slot == EquipmentSlot.Pet)
		{
			base.gameObject.SetActive(value: true);
			Summon component3 = GetComponent<Summon>();
			if ((bool)component3)
			{
				component3.transform.parent = character.transform;
				component3.SkipAnimation = true;
				component3.Launch(character.transform.position + character.transform.forward, null);
			}
			return;
		}
		if (!AttachToSlot(character, slot))
		{
			m_tryEquipAgain = true;
		}
		else
		{
			CreateBoneMapping();
		}
		base.gameObject.SetActive(value: true);
		if (ActiveWhenWielded != null)
		{
			for (int j = 0; j < ActiveWhenWielded.Length; j++)
			{
				if (slot == EquipmentSlot.PrimaryWeapon || slot == EquipmentSlot.SecondaryWeapon)
				{
					ActiveWhenWielded[j].SetActive(value: true);
				}
				else
				{
					ActiveWhenWielded[j].SetActive(value: false);
				}
			}
		}
		ActiveWhenWieldedDelay = false;
		if (!GrimoireSlot)
		{
			return;
		}
		AnimationController component4 = character.GetComponent<AnimationController>();
		if ((bool)component4)
		{
			Animator component5 = GetComponent<Animator>();
			if ((bool)component5 && !component4.SyncList.Contains(component5))
			{
				component5.logWarnings = false;
				component4.SyncList.Add(component5);
			}
		}
	}

	private void h_OnDeath(GameObject myObject, GameEventArgs args)
	{
		Health component = myObject.GetComponent<Health>();
		if (component.ShouldDecay)
		{
			if ((bool)component)
			{
				component.OnDeath -= h_OnDeath;
			}
			m_owner = null;
			m_ownerAlphaControl = null;
			m_ownerEquipment = null;
		}
	}

	private void CreateBoneMapping()
	{
		if (!base.gameObject.activeSelf)
		{
			return;
		}
		AnimationBoneMapper component = m_owner.GetComponent<AnimationBoneMapper>();
		if (!(component != null))
		{
			return;
		}
		component.CreateFXMapping(base.gameObject);
		for (AttackBase.EffectAttachType effectAttachType = AttackBase.EffectAttachType.Fx_Bone_01; effectAttachType <= AttackBase.EffectAttachType.Fx_Bone_10; effectAttachType++)
		{
			if (component.HasBone(base.gameObject, effectAttachType))
			{
				Transform transform = component[base.gameObject, effectAttachType];
				GameObject gameObject = GameUtilities.LaunchLoopingEffect(OnEquipVisualEffect, 1f, transform, null);
				if (gameObject != null)
				{
					m_activeFX.Add(gameObject);
				}
				ApplyItemModEquipFX(transform, m_activeFX);
			}
		}
	}

	private void StoreOriginalTransforms(Transform parentTransform)
	{
		if (parentTransform.transform.parent != null)
		{
			m_originalChildTransforms.Add(parentTransform);
			m_originalParentTransforms.Add(parentTransform.transform.parent);
			SavedBoneTransform item = default(SavedBoneTransform);
			item.Position = parentTransform.localPosition;
			item.Rotation = parentTransform.localRotation;
			item.Scale = parentTransform.localScale;
			m_originalBoneTransforms.Add(item);
		}
		for (int i = 0; i < parentTransform.childCount; i++)
		{
			Transform child = parentTransform.GetChild(i);
			StoreOriginalTransforms(child);
		}
	}

	public bool AttachToSlot(GameObject character, EquipmentSlot slot)
	{
		Transform parent = character.transform;
		Transform transform = base.transform;
		if (base.gameObject.GetComponent<Renderer>() != null)
		{
			base.Renders = true;
			string text = null;
			string text2 = null;
			string value = null;
			Weapon weapon = this as Weapon;
			switch (slot)
			{
			case EquipmentSlot.PrimaryWeapon:
				text = "primaryWeapon";
				text2 = "h_weapon";
				if ((bool)weapon && (weapon.WeaponType == WeaponSpecializationData.WeaponType.HuntingBow || weapon.WeaponType == WeaponSpecializationData.WeaponType.WarBow))
				{
					text = "secondaryWeapon";
				}
				break;
			case EquipmentSlot.Grimoire:
			case EquipmentSlot.SecondaryWeapon:
			{
				Shield component2 = GetComponent<Shield>();
				if ((bool)component2 && component2.ShieldAttachType == Shield.AttachType.Arm)
				{
					text = "LeftForeArm_Att";
					text2 = "l_foreArmWeapon";
				}
				else
				{
					text = "secondaryWeapon";
					text2 = "h_weapon";
				}
				break;
			}
			default:
				if ((bool)weapon && weapon.DisplayWhenAlternate)
				{
					text2 = "a_weapon";
					value = "h_weapon";
					bool flag = false;
					switch (weapon.WeaponType)
					{
					case WeaponSpecializationData.WeaponType.Arbalest:
					case WeaponSpecializationData.WeaponType.Arquebus:
					case WeaponSpecializationData.WeaponType.Crossbow:
					case WeaponSpecializationData.WeaponType.Estoc:
					case WeaponSpecializationData.WeaponType.GreatSword:
					case WeaponSpecializationData.WeaponType.HuntingBow:
					case WeaponSpecializationData.WeaponType.MorningStar:
					case WeaponSpecializationData.WeaponType.Pike:
					case WeaponSpecializationData.WeaponType.Pollaxe:
					case WeaponSpecializationData.WeaponType.Quarterstaff:
					case WeaponSpecializationData.WeaponType.Spear:
					case WeaponSpecializationData.WeaponType.WarBow:
						flag = true;
						break;
					}
					if (weapon.BackScabbardOverride)
					{
						flag = true;
					}
					switch (slot)
					{
					case EquipmentSlot.PrimaryWeapon2:
						text = ((!flag) ? "primaryScabbard" : "backScabbard");
						break;
					case EquipmentSlot.SecondaryWeapon2:
					{
						Shield component = GetComponent<Shield>();
						text = ((!flag && (!component || component.ShieldAttachType != 0)) ? "secondaryScabbard" : "backScabbard");
						break;
					}
					}
				}
				break;
			}
			if (text != null)
			{
				base.gameObject.SetActive(value: true);
				base.Renders = true;
				AnimationController component3 = character.GetComponent<AnimationController>();
				NPCAppearance component4 = character.GetComponent<NPCAppearance>();
				if ((bool)component3)
				{
					Transform transform2 = null;
					if ((bool)component4)
					{
						transform2 = component3.GetBoneTransform(text + "Scaled", character.transform);
					}
					if (transform2 == null)
					{
						transform2 = component3.GetBoneTransform(text, character.transform);
					}
					if (transform2 != null)
					{
						parent = transform2;
					}
					else
					{
						transform2 = component3.GetBoneTransform((slot == EquipmentSlot.PrimaryWeapon) ? HumanBodyBones.RightHand : HumanBodyBones.LeftHand);
						if (!(transform2 != null))
						{
							Debug.LogWarning(character.name + " doesn't have the specified bone (" + text + ") for Weapon Attachment.", character);
							base.gameObject.SetActive(value: true);
							base.Renders = true;
							transform.parent = m_owner.transform;
							transform.localPosition = Vector3.zero;
							transform.localRotation = Quaternion.identity;
							transform.localScale = Vector3.one;
							return false;
						}
						parent = transform2;
					}
				}
				Transform transform3 = FindBone(base.gameObject.transform, text2);
				if (transform3 == null && !string.IsNullOrEmpty(value))
				{
					transform3 = FindBone(base.gameObject.transform, value);
				}
				if ((bool)transform3)
				{
					transform3.parent = base.gameObject.transform;
					Transform[] array = new Transform[base.gameObject.transform.childCount];
					for (int i = 0; i < base.gameObject.transform.childCount; i++)
					{
						Transform transform4 = (array[i] = base.gameObject.transform.GetChild(i));
					}
					for (int j = 0; j < array.Length; j++)
					{
						if (array[j] != transform3)
						{
							array[j].transform.parent = transform3;
						}
					}
					transform3.localPosition = Vector3.zero;
					transform3.localRotation = Quaternion.identity;
				}
			}
			else
			{
				base.Renders = false;
			}
		}
		base.gameObject.layer = character.layer;
		transform.parent = parent;
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
		transform.localScale = Vector3.one;
		m_tryEquipAgain = false;
		return true;
	}

	public override void Restored()
	{
		base.Restored();
		base.transform.localPosition = Vector3.zero;
		base.transform.localRotation = Quaternion.identity;
		foreach (ItemMod item in m_modsSerialized)
		{
			if (!(item != null))
			{
				continue;
			}
			if (item.IsQualityMod)
			{
				foreach (ItemModComponent attachedItemMod in AttachedItemMods)
				{
					if (attachedItemMod.Mod.IsQualityMod)
					{
						DestroyItemMod(attachedItemMod);
						break;
					}
				}
			}
			AttachItemMod(item);
		}
		m_modsSerialized.Clear();
	}

	public virtual Equippable UnEquip(GameObject character, EquipmentSlot slot)
	{
		if (GrimoireSlot)
		{
			AnimationController component = character.GetComponent<AnimationController>();
			if ((bool)component)
			{
				Animator component2 = GetComponent<Animator>();
				if ((bool)component2)
				{
					component.SyncList.Remove(component2);
				}
			}
		}
		AnimationBoneMapper component3 = character.GetComponent<AnimationBoneMapper>();
		if (component3 != null)
		{
			component3.ClearMapping(base.gameObject);
		}
		if ((bool)m_owner)
		{
			Health component4 = m_owner.GetComponent<Health>();
			if ((bool)component4)
			{
				component4.OnDeath -= h_OnDeath;
			}
		}
		if (slot != EquipmentSlot.PrimaryWeapon2 && slot != EquipmentSlot.SecondaryWeapon2)
		{
			SendMessage("OnUnequip", character, SendMessageOptions.DontRequireReceiver);
		}
		m_owner = null;
		m_ownerAlphaControl = null;
		m_ownerEquipment = null;
		m_equipped_slot = EquipmentSlot.None;
		CharacterStats component5 = character.GetComponent<CharacterStats>();
		if (component5 != null)
		{
			component5.ClearEffectInSlot(slot);
		}
		RemoveItemModEquipEffects(character);
		foreach (GameObject item in m_activeFX)
		{
			GameUtilities.Destroy(item);
		}
		m_activeFX.Clear();
		if (m_originalParentTransforms != null && base.Location != 0)
		{
			for (int i = 0; i < m_originalParentTransforms.Count; i++)
			{
				if ((bool)m_originalChildTransforms[i])
				{
					m_originalChildTransforms[i].transform.parent = m_originalParentTransforms[i];
					m_originalChildTransforms[i].transform.localPosition = m_originalBoneTransforms[i].Position;
					m_originalChildTransforms[i].transform.localRotation = m_originalBoneTransforms[i].Rotation;
					m_originalChildTransforms[i].transform.localScale = m_originalBoneTransforms[i].Scale;
				}
				else
				{
					Debug.LogError("An equipment transform has gone missing. '" + base.name + "' may be improperly sized.");
				}
			}
		}
		if (slot == EquipmentSlot.Pet && (bool)character.GetComponent<AIController>())
		{
			Summon component6 = GetComponent<Summon>();
			if (component6 == null || component6.GetNumSummons() == 0)
			{
				return this;
			}
			component6.DeactivateSummons(deactivateAbility: false);
		}
		if (ActiveWhenWielded != null && ActiveWhenWielded.Length != 0)
		{
			ActiveWhenWieldedDelay = true;
		}
		return this;
	}

	public void Deteriorate()
	{
		GameObject gameObject = base.gameObject;
		while (gameObject.transform.parent != null)
		{
			gameObject = gameObject.transform.parent.gameObject;
		}
		float num = 1f;
		CharacterStats component = gameObject.GetComponent<CharacterStats>();
		if ((bool)component)
		{
			float num2 = 1f - (float)component.CalculateSkill(CharacterStats.SkillType.Survival) * 0.1f;
			if (num2 < 0f)
			{
				num2 = 0f;
			}
			num *= num2;
		}
		if (m_durability < 0f)
		{
			m_durability = 0f;
		}
	}

	public void ResetDurability()
	{
		m_durability = MaxDurability;
	}

	private Transform FindBone(Transform current, string name)
	{
		if (current.name == name)
		{
			return current;
		}
		for (int i = 0; i < current.childCount; i++)
		{
			Transform transform = FindBone(current.GetChild(i), name);
			if (transform != null)
			{
				return transform;
			}
		}
		return null;
	}

	public virtual float FindLaunchAccuracyBonus(AttackBase attack)
	{
		float num = 0f;
		for (int i = 0; i < m_itemModComponents.Count; i++)
		{
			ItemModComponent itemModComponent = m_itemModComponents[i];
			num += itemModComponent.FindLaunchAccuracyBonus(attack);
		}
		return num;
	}

	public virtual void AdjustDamageForUi(GameObject character, DamageInfo damage)
	{
		foreach (ItemModComponent attachedItemMod in AttachedItemMods)
		{
			attachedItemMod.AdjustDamageForUi(base.gameObject, damage);
		}
	}

	public virtual void ApplyLaunchEffects(GameObject parent)
	{
		for (int i = 0; i < m_itemModComponents.Count; i++)
		{
			m_itemModComponents[i].ApplyLaunchEffects(parent, this);
		}
	}

	public void ApplyItemModAttackEffects(GameObject owner, CharacterStats enemyStats, DamageInfo info, List<StatusEffect> appliedEffects)
	{
		for (int i = 0; i < m_itemModComponents.Count; i++)
		{
			m_itemModComponents[i].ApplyAttackEffects(owner, enemyStats, info, this, appliedEffects);
		}
	}

	public void ApplyItemModDamageProcs(DamageInfo damage)
	{
		for (int i = 0; i < m_itemModComponents.Count; i++)
		{
			m_itemModComponents[i].ApplyDamageProcs(damage);
		}
	}

	public void ApplyItemModEquipEffects(GameObject character, EquipmentSlot slot, GenericAbility.AbilityType abType)
	{
		for (int i = 0; i < m_itemModComponents.Count; i++)
		{
			m_itemModComponents[i].ApplyEquipEffects(character, slot, abType, this);
		}
	}

	public void ApplyItemModEquipFX(Transform t, List<GameObject> fx)
	{
		for (int i = 0; i < m_itemModComponents.Count; i++)
		{
			m_itemModComponents[i].ApplyEquipFX(t, fx);
		}
	}

	public void RemoveItemModEquipEffects(GameObject character)
	{
		for (int i = 0; i < m_itemModComponents.Count; i++)
		{
			m_itemModComponents[i].RemoveEquipEffects(character, this);
		}
	}

	public void HandleItemModsOnSpiritshift(GameObject character)
	{
		for (int i = 0; i < m_itemModComponents.Count; i++)
		{
			m_itemModComponents[i].HandleSpiritshift(character);
		}
	}

	public AttackBase FindItemModSecondaryAttack()
	{
		for (int i = 0; i < m_itemModComponents.Count; i++)
		{
			AttackBase secondaryAttack = m_itemModComponents[i].SecondaryAttack;
			if (secondaryAttack != null)
			{
				secondaryAttack.ValidateOwnerStats(m_owner);
				return secondaryAttack;
			}
		}
		return null;
	}

	private void StartListeningForChargedUsage()
	{
		for (int i = 0; i < m_itemModComponents.Count; i++)
		{
			ItemModComponent itemModComponent = m_itemModComponents[i];
			if (itemModComponent.Charged)
			{
				itemModComponent.ChargeAbility.OnCooldown += HandleChargedUsage;
			}
		}
		m_listening_for_charged_usage = true;
	}

	private void StopListeningForChargedUsage()
	{
		for (int i = 0; i < m_itemModComponents.Count; i++)
		{
			ItemModComponent itemModComponent = m_itemModComponents[i];
			if (itemModComponent.Charged)
			{
				itemModComponent.ChargeAbility.OnCooldown -= HandleChargedUsage;
			}
		}
		m_listening_for_charged_usage = false;
	}

	private void HandleChargedUsage(GameObject source)
	{
		for (int i = 0; i < m_itemModComponents.Count; i++)
		{
			ItemModComponent itemModComponent = m_itemModComponents[i];
			if (!itemModComponent.Charged || itemModComponent.UsesLeft() > 0)
			{
				return;
			}
		}
		UnEquip(m_owner, m_equipped_slot);
		GameUtilities.Destroy(this, 1f);
	}

	public static GenericAbility.AbilityType EquipmentSlotToAbilityType(EquipmentSlot slot)
	{
		switch (slot)
		{
		case EquipmentSlot.RightRing:
		case EquipmentSlot.LeftRing:
			return GenericAbility.AbilityType.Ring;
		case EquipmentSlot.PrimaryWeapon:
		case EquipmentSlot.SecondaryWeapon:
			return GenericAbility.AbilityType.WeaponOrShield;
		default:
			return GenericAbility.AbilityType.Equipment;
		}
	}

	public string GetUniqueString()
	{
		if ((bool)GetComponent<EquipmentSoulbind>())
		{
			return GUIUtils.GetText(2044);
		}
		if (this is Weapon && ((Weapon)this).UniversalType)
		{
			return GUIUtils.GetText(2039);
		}
		if (Unique)
		{
			return StringTableManager.GetText(DatabaseString.StringTableType.Recipes, 6);
		}
		return string.Empty;
	}

	public override string GetTooltipContent(GameObject owner)
	{
		if (this == null || base.gameObject == null)
		{
			return "";
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (!GetComponent<Shield>())
		{
			string value = UIItemInspectManager.GetEquippableItemType(this, owner, this).Trim();
			if (!string.IsNullOrEmpty(value))
			{
				stringBuilder.AppendLine(value);
			}
		}
		string value2 = GetString(owner).Trim();
		if (!string.IsNullOrEmpty(value2))
		{
			stringBuilder.AppendLine(value2);
		}
		if (stringBuilder.Length > 0)
		{
			stringBuilder.AppendLine();
		}
		stringBuilder.Append("[" + NGUITools.EncodeColor(AttackBase.StringKeyColor) + "]" + GUIUtils.GetText(1796) + "[-]");
		return stringBuilder.ToString();
	}

	public override string GetString(GameObject owner)
	{
		StringBuilder stringBuilder = new StringBuilder();
		StringEffects stringEffects = new StringEffects();
		Weapon weapon = this as Weapon;
		IEnumerable<StatusEffectParams> first = new List<StatusEffectParams>();
		if (base.IsPrefab)
		{
			ItemMod[] itemMods = ItemMods;
			foreach (ItemMod itemMod2 in itemMods)
			{
				if ((bool)itemMod2)
				{
					first = first.Concat(itemMod2.StatusEffectsOnLaunch);
					first = first.Concat(itemMod2.StatusEffectsOnEquip);
				}
			}
		}
		else
		{
			foreach (ItemModComponent attachedItemMod in AttachedItemMods)
			{
				if ((bool)attachedItemMod)
				{
					first = first.Concat(attachedItemMod.Mod.StatusEffectsOnLaunch);
					first = first.Concat(attachedItemMod.Mod.StatusEffectsOnEquip);
				}
			}
		}
		first = first.Concat(StatusEffects);
		if ((bool)weapon && weapon.StatusEffectsOnLaunch != null)
		{
			first = first.Concat(weapon.StatusEffectsOnLaunch);
		}
		AttackBase component = GetComponent<AttackBase>();
		bool flag = false;
		if (component is Summon)
		{
			flag = ((Summon)component).SummonType == AIController.AISummonType.Pet;
		}
		if ((bool)component && !flag)
		{
			stringBuilder.AppendLine(component.GetString(null, owner, stringEffects, first));
		}
		string value = AttackBase.StringEffects(stringEffects, targets: true);
		if (!string.IsNullOrEmpty(value))
		{
			stringBuilder.AppendLine(value);
		}
		Armor component2 = GetComponent<Armor>();
		if ((bool)component2)
		{
			float num = 0f;
			foreach (StatusEffectParams item in first)
			{
				if (item.AffectsStat == StatusEffect.ModifiedStat.BonusDTFromArmor || (item.AffectsStat == StatusEffect.ModifiedStat.DamageThreshhold && item.DmgType == DamagePacket.DamageType.All))
				{
					num += item.GetValue(owner);
				}
			}
			float num2 = component2.GetDamageThreshold(owner) + num;
			stringBuilder.Append(AttackBase.FormatWC(GUIUtils.GetText(1543), num2));
			if (UiDetailedContent)
			{
				List<string> list = new List<string>();
				for (int j = 0; j < 7; j++)
				{
					float num3 = num2;
					foreach (StatusEffectParams item2 in first)
					{
						if (item2.AffectsStat == StatusEffect.ModifiedStat.DamageThreshhold && item2.DmgType == (DamagePacket.DamageType)j)
						{
							num3 += item2.GetValue(owner);
						}
					}
					num3 = component2.AdjustForDamageType(num3, (DamagePacket.DamageType)j);
					if (float.IsPositiveInfinity(num3))
					{
						list.Add(GUIUtils.GetDamageTypeString((DamagePacket.DamageType)j) + ": " + GUIUtils.GetText(2187));
					}
					else if (num3 != num2 || component2.AdjustForDamageType(1f, (DamagePacket.DamageType)j) != 1f)
					{
						list.Add(GUIUtils.GetDamageTypeString((DamagePacket.DamageType)j) + ": " + num3.ToString("#0"));
					}
				}
				if (list.Count > 0)
				{
					stringBuilder.AppendGuiFormat(1731, TextUtils.FuncJoin((string s) => s, list, GUIUtils.Comma()));
				}
			}
			stringBuilder.AppendLine();
			stringBuilder.AppendLine(AttackBase.FormatWC(GUIUtils.GetText(1542), GUIUtils.Format(1277, (100f * (component2.SpeedFactor - 1f)).ToString("####0"))));
		}
		Shield component3 = GetComponent<Shield>();
		if ((bool)component3)
		{
			stringBuilder.AppendLine(GUIUtils.Format(1421, GUIUtils.GetShieldCategoryString(component3.ShieldCategory)));
			stringBuilder.AppendLine(AttackBase.FormatWC(GUIUtils.GetText(1108), component3.DeflectBonus));
			if (component3.ReflexBonus != 0)
			{
				stringBuilder.AppendLine(AttackBase.FormatWC(GUIUtils.GetText(1110), TextUtils.NumberBonus(component3.ReflexBonus)));
			}
			if (component3.AccuracyBonus != 0)
			{
				stringBuilder.AppendLine(AttackBase.FormatWC(GUIUtils.GetText(1404), TextUtils.NumberBonus(component3.AccuracyBonus)));
			}
		}
		bool flag2 = !ArrayExtender.IsNullOrEmpty(StatusEffects);
		bool flag3 = (bool)weapon && !ArrayExtender.IsNullOrEmpty(weapon.StatusEffectsOnLaunch);
		if (flag2 || flag3)
		{
			stringBuilder.AppendLine(GUIUtils.GetText(1604));
			stringBuilder.Append('\r');
			stringBuilder.Append(TARGET_USER.GetText() + ": ");
			if (flag2)
			{
				stringBuilder.AppendLine(StatusEffectParams.ListToString(StatusEffects, null));
			}
			if (flag3)
			{
				stringBuilder.AppendLine(StatusEffectParams.ListToString(weapon.StatusEffectsOnLaunch, null));
			}
		}
		if (base.IsPrefab)
		{
			if (!UiDetailedContent && ItemMods.Length != 0)
			{
				stringBuilder.AppendLine(AttackBase.FormatWC(GUIUtils.GetText(1876), TextUtils.FuncJoin((ItemMod itemMod) => itemMod.DisplayName.GetText(), ItemMods.Where((ItemMod itemMod) => itemMod), GUIUtils.Comma())));
			}
		}
		else if (!UiDetailedContent && AttachedItemMods.Count > 0)
		{
			stringBuilder.AppendLine(AttackBase.FormatWC(GUIUtils.GetText(1876), TextUtils.FuncJoin((ItemModComponent ic) => ic.Mod.DisplayName.GetText(), AttachedItemMods, ", ")));
		}
		stringBuilder.AppendLine(base.GetString(owner));
		return stringBuilder.ToString().Trim();
	}
}
