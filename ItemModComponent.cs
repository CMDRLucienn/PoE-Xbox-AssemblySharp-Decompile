using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

[DebuggerDisplay("{GetDebuggerString()} (ItemModComponent)")]
public class ItemModComponent : MonoBehaviour
{
	private ItemMod m_mod;

	private Equippable m_Owner;

	private GenericAbility m_ability;

	private AttackBase m_attack;

	public GenericAbility Ability => m_ability;

	public ItemMod Mod
	{
		get
		{
			return m_mod;
		}
		set
		{
			m_mod = value;
		}
	}

	public AttackBase SecondaryAttack
	{
		get
		{
			if (m_mod.AbilityTriggeredOn == ItemMod.TriggerMode.AsSecondaryAttack)
			{
				return m_attack;
			}
			return null;
		}
	}

	public bool Charged
	{
		get
		{
			if (m_ability != null && m_ability.CooldownType == GenericAbility.CooldownMode.Charged)
			{
				return true;
			}
			return false;
		}
	}

	public GenericAbility ChargeAbility
	{
		get
		{
			if (m_ability != null && m_ability.CooldownType == GenericAbility.CooldownMode.Charged)
			{
				return m_ability;
			}
			return null;
		}
	}

	public string GetDebuggerString()
	{
		return base.name + ": " + m_mod.name;
	}

	public static ItemModComponent Create(Equippable item, ItemMod mod)
	{
		ItemModComponent itemModComponent = item.gameObject.AddComponent<ItemModComponent>();
		itemModComponent.Initialize(item, mod);
		return itemModComponent;
	}

	private void OnDestroy()
	{
		if ((bool)m_Owner)
		{
			m_Owner.ItemModsChanged -= OnOwnerItemModsChanged;
		}
		if ((bool)m_ability)
		{
			GameUtilities.Destroy(m_ability.gameObject);
		}
		ComponentUtils.NullOutObjectReferences(this);
	}

	public void Initialize(Equippable item, ItemMod mod)
	{
		m_mod = mod;
		m_Owner = item;
		if (!(mod.AbilityPrefab != null))
		{
			return;
		}
		item.ItemModsChanged += OnOwnerItemModsChanged;
		for (int i = 0; i < item.AbilityModGuidsSerialized.Count; i++)
		{
			GameObject objectByID = InstanceID.GetObjectByID(item.AbilityModGuidsSerialized[i]);
			if ((bool)objectByID)
			{
				GenericAbility component = objectByID.GetComponent<GenericAbility>();
				if ((bool)component && GenericAbility.NameComparer.Instance.Equals(component, mod.AbilityPrefab))
				{
					m_ability = component;
					break;
				}
			}
		}
		if (m_ability == null)
		{
			m_ability = GameResources.Instantiate<GenericAbility>(mod.AbilityPrefab);
			m_ability.AppliedViaMod = true;
			if ((bool)m_ability)
			{
				InstanceID component2 = m_ability.GetComponent<InstanceID>();
				if ((bool)component2)
				{
					item.AbilityModGuidsSerialized.Add(component2.Guid);
				}
			}
		}
		m_attack = m_ability.GetComponent<AttackBase>();
		AttackBase component3 = item.GetComponent<AttackBase>();
		if (component3 != null)
		{
			SubscribeToAttack(component3);
		}
		GenericSpell genericSpell = m_ability as GenericSpell;
		if ((bool)genericSpell)
		{
			genericSpell.NeedsGrimoire = false;
			genericSpell.IsFree = true;
		}
	}

	private void SubscribeToAttack(AttackBase attack)
	{
		if (!(attack == null))
		{
			if (m_mod.AbilityTriggeredOn == ItemMod.TriggerMode.OnScoringCriticalHit)
			{
				attack.OnAttackRollCalculated -= HandleAttackOnScoringCriticalHit;
				attack.OnAttackRollCalculated += HandleAttackOnScoringCriticalHit;
			}
			else if (m_mod.AbilityTriggeredOn == ItemMod.TriggerMode.OnScoringCritOrHit)
			{
				attack.OnAttackRollCalculated -= HandleAttackOnScoringHit;
				attack.OnAttackRollCalculated += HandleAttackOnScoringHit;
			}
			else if (m_mod.AbilityTriggeredOn == ItemMod.TriggerMode.OnScoringKill)
			{
				attack.OnKill -= HandleAttackOnKill;
				attack.OnKill += HandleAttackOnKill;
			}
		}
	}

	private void OnOwnerItemModsChanged(Equippable owner)
	{
		foreach (ItemModComponent attachedItemMod in owner.AttachedItemMods)
		{
			if ((bool)attachedItemMod && attachedItemMod != this && attachedItemMod.Mod.AbilityTriggeredOn == ItemMod.TriggerMode.AsSecondaryAttack)
			{
				AttackBase attackBase = null;
				if ((bool)attachedItemMod.Ability)
				{
					attackBase = ((!attachedItemMod.Ability.Attack) ? attachedItemMod.Ability.GetComponent<AttackBase>() : attachedItemMod.Ability.Attack);
				}
				if ((bool)attackBase)
				{
					SubscribeToAttack(attackBase);
				}
			}
		}
	}

	public float FindLaunchAccuracyBonus(AttackBase attack)
	{
		float num = 0f;
		StatusEffectParams[] statusEffectsOnLaunch = m_mod.StatusEffectsOnLaunch;
		foreach (StatusEffectParams statusEffectParams in statusEffectsOnLaunch)
		{
			if (statusEffectParams != null)
			{
				num += statusEffectParams.EstimateAccuracyBonusForUi(attack);
			}
		}
		return num;
	}

	public void AdjustDamageForUi(GameObject character, DamageInfo damage)
	{
		StatusEffectParams[] statusEffectsOnLaunch = Mod.StatusEffectsOnLaunch;
		for (int i = 0; i < statusEffectsOnLaunch.Length; i++)
		{
			statusEffectsOnLaunch[i]?.AdjustDamageForUi(character, damage);
		}
	}

	public void ApplyLaunchEffects(GameObject parent, Equippable equip)
	{
		CharacterStats component = parent.GetComponent<CharacterStats>();
		if (!(component != null))
		{
			return;
		}
		StatusEffectParams[] statusEffectsOnLaunch = m_mod.StatusEffectsOnLaunch;
		foreach (StatusEffectParams statusEffectParams in statusEffectsOnLaunch)
		{
			if (statusEffectParams != null)
			{
				StatusEffect statusEffect = StatusEffect.Create(parent, equip, statusEffectParams, GenericAbility.AbilityType.WeaponOrShield, null, deleteOnClear: true);
				statusEffect.FriendlyRadius = m_mod.FriendlyRadius;
				component.ApplyStatusEffectImmediate(statusEffect);
			}
		}
	}

	public void ApplyAttackEffects(GameObject owner, CharacterStats enemyStats, DamageInfo info, Equippable equip, List<StatusEffect> appliedEffects)
	{
		StatusEffectParams[] statusEffectsOnAttack = m_mod.StatusEffectsOnAttack;
		foreach (StatusEffectParams statusEffectParams in statusEffectsOnAttack)
		{
			if (statusEffectParams != null)
			{
				StatusEffect statusEffect = StatusEffect.Create(owner, equip, statusEffectParams, GenericAbility.AbilityType.WeaponOrShield, info, deleteOnClear: true);
				statusEffect.FriendlyRadius = m_mod.FriendlyRadius;
				if (enemyStats.ApplyStatusEffectImmediate(statusEffect))
				{
					appliedEffects?.Add(statusEffect);
				}
			}
		}
	}

	public void ApplyDamageProcs(DamageInfo damage)
	{
		DamagePacket.DamageProcType[] damageProcs = m_mod.DamageProcs;
		foreach (DamagePacket.DamageProcType damageProcType in damageProcs)
		{
			if (damageProcType != null)
			{
				DamagePacket.DamageProcType item = new DamagePacket.DamageProcType(damageProcType.Type, damageProcType.PercentOfBaseDamage);
				damage.Damage.DamageProc.Add(item);
			}
		}
	}

	public void ApplyEquipEffects(GameObject character, Equippable.EquipmentSlot slot, GenericAbility.AbilityType abType, Equippable equip)
	{
		if (PE_Paperdoll.IsObjectPaperdoll(character) || slot == Equippable.EquipmentSlot.PrimaryWeapon2 || slot == Equippable.EquipmentSlot.SecondaryWeapon2)
		{
			return;
		}
		CharacterStats component = character.GetComponent<CharacterStats>();
		if (component != null)
		{
			StatusEffectParams[] statusEffectsOnEquip = m_mod.StatusEffectsOnEquip;
			foreach (StatusEffectParams statusEffectParams in statusEffectsOnEquip)
			{
				if (statusEffectParams != null)
				{
					StatusEffect statusEffect = StatusEffect.Create(character, equip, statusEffectParams, abType, null, deleteOnClear: false);
					statusEffect.FriendlyRadius = m_mod.FriendlyRadius;
					statusEffect.Slot = slot;
					component.ApplyStatusEffect(statusEffect);
				}
			}
		}
		if (m_mod.AbilityModsOnEquip != null && component != null)
		{
			GenericAbility[] abilitiesToModOnEquip = m_mod.AbilitiesToModOnEquip;
			foreach (GenericAbility a in abilitiesToModOnEquip)
			{
				foreach (GenericAbility activeAbility in component.ActiveAbilities)
				{
					if (GenericAbility.NameComparer.Instance.Equals(a, activeAbility))
					{
						AbilityMod[] abilityModsOnEquip = m_mod.AbilityModsOnEquip;
						foreach (AbilityMod mod in abilityModsOnEquip)
						{
							activeAbility.AddAbilityMod(mod, abType, equip);
						}
					}
				}
			}
		}
		Spiritshift spiritshift = m_ability as Spiritshift;
		if ((bool)spiritshift)
		{
			spiritshift.AddSpiritshiftFormAbilities(component);
		}
		if (m_ability != null)
		{
			if (!m_ability.Icon)
			{
				m_ability.Icon = equip.IconTexture;
			}
			m_ability.Owner = character;
			m_ability.EffectType = abType;
			switch (m_mod.AbilityTriggeredOn)
			{
			case ItemMod.TriggerMode.OnUI:
				if (component != null && !component.ActiveAbilities.Contains(m_ability))
				{
					component.ActiveAbilities.Add(m_ability);
				}
				break;
			case ItemMod.TriggerMode.OnBeingCriticallyHit:
			case ItemMod.TriggerMode.OnBeingCritOrHit:
			case ItemMod.TriggerMode.OnStaminaBelowRatio:
			{
				Health component3 = character.GetComponent<Health>();
				if (component3 != null)
				{
					component3.OnDamaged += HandleHealthOnDamaged;
				}
				break;
			}
			case ItemMod.TriggerMode.OnUnconscious:
			{
				Health component4 = character.GetComponent<Health>();
				if (component4 != null)
				{
					component4.OnUnconscious += HandleHealthOnUnconscious;
				}
				break;
			}
			case ItemMod.TriggerMode.OnScoringKill:
				if (!equip.GetComponent<AttackBase>())
				{
					Health component2 = character.GetComponent<Health>();
					if (component2 != null)
					{
						component2.OnKill += HandleHealthOnKill;
					}
				}
				break;
			}
		}
		if ((bool)m_attack)
		{
			m_attack.Owner = character;
			m_attack.ResetLaunchTransforms();
		}
	}

	public void ApplyEquipFX(Transform t, List<GameObject> fx_list)
	{
		if (m_mod.OnEquipVisualEffect != null)
		{
			GameObject gameObject = GameUtilities.LaunchLoopingEffect(m_mod.OnEquipVisualEffect, 1f, t, null);
			if (gameObject != null)
			{
				fx_list?.Add(gameObject);
			}
		}
	}

	public void RemoveEquipEffects(GameObject character, Equippable equip)
	{
		if (PE_Paperdoll.IsObjectPaperdoll(character))
		{
			return;
		}
		CharacterStats component = character.GetComponent<CharacterStats>();
		Spiritshift spiritshift = m_ability as Spiritshift;
		if ((bool)spiritshift)
		{
			spiritshift.RemoveSpiritshiftFormAbilities(component);
		}
		if (m_mod.AbilityModsOnEquip != null && component != null)
		{
			GenericAbility[] abilitiesToModOnEquip = m_mod.AbilitiesToModOnEquip;
			foreach (GenericAbility a in abilitiesToModOnEquip)
			{
				foreach (GenericAbility activeAbility in component.ActiveAbilities)
				{
					if (GenericAbility.NameComparer.Instance.Equals(a, activeAbility))
					{
						AbilityMod[] abilityModsOnEquip = m_mod.AbilityModsOnEquip;
						foreach (AbilityMod mod in abilityModsOnEquip)
						{
							activeAbility.RemoveAbilityModByOrigin(mod, equip);
						}
					}
				}
			}
		}
		if (m_ability != null && m_mod.AbilityTriggeredOn != ItemMod.TriggerMode.OnSpiritShift)
		{
			if (m_ability.Activated)
			{
				m_ability.ForceDeactivate(null);
			}
			switch (m_mod.AbilityTriggeredOn)
			{
			case ItemMod.TriggerMode.OnUI:
				if (component != null)
				{
					component.ActiveAbilities.Remove(m_ability);
				}
				break;
			case ItemMod.TriggerMode.OnBeingCriticallyHit:
			case ItemMod.TriggerMode.OnBeingCritOrHit:
			case ItemMod.TriggerMode.OnStaminaBelowRatio:
			{
				Health component3 = character.GetComponent<Health>();
				if (component3 != null)
				{
					component3.OnDamaged -= HandleHealthOnDamaged;
				}
				break;
			}
			case ItemMod.TriggerMode.OnUnconscious:
			{
				Health component4 = character.GetComponent<Health>();
				if (component4 != null)
				{
					component4.OnUnconscious -= HandleHealthOnUnconscious;
				}
				break;
			}
			case ItemMod.TriggerMode.OnScoringKill:
			{
				Health component2 = character.GetComponent<Health>();
				if (component2 != null)
				{
					component2.OnKill -= HandleHealthOnKill;
				}
				break;
			}
			}
		}
		if (m_attack != null && m_attack is AttackPulsedAOE)
		{
			if (m_ability != null)
			{
				m_ability.Owner = null;
			}
			m_attack.Owner = null;
		}
	}

	private bool ShouldTriggerAbility()
	{
		if (m_ability != null && m_ability.ReadyIgnoreRecovery)
		{
			return OEIRandom.FloatValue() < m_mod.AbilityTriggerChance;
		}
		return false;
	}

	private void HandleAttackOnScoringHit(GameObject myObject, CombatEventArgs args)
	{
		if (args.Damage.HitType != 0 && args.Damage.HitType != HitType.GRAZE && ShouldTriggerAbility())
		{
			if (m_mod.AbilityTarget == ItemMod.TargetMode.Self)
			{
				StartUse(myObject, myObject);
			}
			else
			{
				StartUse(myObject, args.Victim);
			}
			if ((bool)m_ability)
			{
				m_ability.UpdateStatusEffectActivation();
			}
		}
	}

	private void HandleAttackOnScoringCriticalHit(GameObject myObject, CombatEventArgs args)
	{
		if (args.Damage.HitType == HitType.CRIT && ShouldTriggerAbility())
		{
			if (m_mod.AbilityTarget == ItemMod.TargetMode.Self)
			{
				StartUse(myObject, myObject);
			}
			else
			{
				StartUse(myObject, args.Victim);
			}
			if ((bool)m_ability)
			{
				m_ability.UpdateStatusEffectActivation();
			}
		}
	}

	private void HandleAttackOnKill(GameObject myObject, CombatEventArgs args)
	{
		if (ShouldTriggerAbility())
		{
			if (m_mod.AbilityTarget == ItemMod.TargetMode.Self)
			{
				StartUse(myObject, myObject);
			}
			else
			{
				StartUse(myObject, args.Victim);
			}
		}
	}

	private void HandleHealthOnKill(GameObject myObject, GameEventArgs args)
	{
		if (ShouldTriggerAbility())
		{
			if (m_mod.AbilityTarget == ItemMod.TargetMode.Self)
			{
				StartUse(myObject, myObject);
			}
			else if (args.GameObjectData != null && args.GameObjectData.Length != 0)
			{
				StartUse(myObject, args.GameObjectData[0]);
			}
		}
	}

	private void HandleHealthOnUnconscious(GameObject myObject, GameEventArgs args)
	{
		Health health = (myObject ? myObject.GetComponent<Health>() : null);
		if (!ShouldTriggerAbility())
		{
			return;
		}
		if (m_mod.AbilityTarget == ItemMod.TargetMode.Self)
		{
			if ((bool)health)
			{
				health.SuspendDecay(enabled: true);
			}
			StartCoroutine(UseDelay(myObject, myObject));
			return;
		}
		AIController component = myObject.GetComponent<AIController>();
		if (component != null)
		{
			GameObject currentTarget = component.CurrentTarget;
			if (currentTarget != null)
			{
				StartCoroutine(UseDelay(myObject, currentTarget));
			}
		}
	}

	private void HandleHealthOnDamaged(GameObject myObject, GameEventArgs args)
	{
		if (!ShouldTriggerAbility())
		{
			return;
		}
		DamageInfo damageInfo = (DamageInfo)args.GenericData[0];
		bool flag = false;
		if (m_mod.AbilityTriggeredOn == ItemMod.TriggerMode.OnBeingCritOrHit)
		{
			flag = damageInfo != null && (damageInfo.IsPlainHit || damageInfo.IsCriticalHit);
		}
		else if (m_mod.AbilityTriggeredOn == ItemMod.TriggerMode.OnBeingCriticallyHit)
		{
			flag = damageInfo?.IsCriticalHit ?? false;
		}
		else if (m_mod.AbilityTriggeredOn == ItemMod.TriggerMode.OnStaminaBelowRatio)
		{
			Health health = (myObject ? myObject.GetComponent<Health>() : null);
			if ((bool)health)
			{
				flag = args.FloatData[1] / health.MaxStamina >= m_mod.AbilityTriggerValue && health.StaminaPercentage < m_mod.AbilityTriggerValue;
			}
		}
		if (flag)
		{
			if (m_mod.AbilityTarget == ItemMod.TargetMode.Self)
			{
				StartUse(myObject, myObject);
			}
			else
			{
				StartUse(myObject, args.GameObjectData[0]);
			}
		}
	}

	public void HandleSpiritshift(GameObject owner)
	{
		if (m_mod.AbilityTriggeredOn == ItemMod.TriggerMode.OnSpiritShift && ShouldTriggerAbility())
		{
			StartUse(owner, owner);
		}
	}

	public void StartUse(GameObject owner, GameObject enemy)
	{
		if (m_ability != null && owner != null && enemy != null)
		{
			m_ability.transform.parent = owner.transform;
			m_ability.Owner = owner;
			if ((bool)m_ability)
			{
				m_ability.ActivateIgnoreRecovery(enemy);
			}
			else
			{
				m_attack.Launch(enemy, m_ability);
			}
		}
	}

	private IEnumerator UseDelay(GameObject owner, GameObject enemy)
	{
		yield return new WaitForSeconds(1f);
		if ((bool)m_ability && m_ability.ReadyIgnoreRecovery)
		{
			StartUse(owner, enemy);
		}
		Health health = (owner ? owner.GetComponent<Health>() : null);
		if ((bool)health)
		{
			health.SuspendDecay(enabled: false);
		}
	}

	public int UsesLeft()
	{
		if (m_ability != null)
		{
			return m_ability.UsesLeft();
		}
		return int.MaxValue;
	}

	public bool IsInstanceOf(ItemMod mod)
	{
		if ((bool)mod)
		{
			return mod.Equals(m_mod);
		}
		return false;
	}
}
