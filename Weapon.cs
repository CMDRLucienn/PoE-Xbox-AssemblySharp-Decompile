using UnityEngine;

public class Weapon : Equippable
{
	public enum Stance
	{
		Unarmed,
		OneHanded,
		TwoHandedClosedGrip,
		TwoHandedOpenGrip
	}

	public StatusEffectParams[] StatusEffectsOnLaunch;

	[Tooltip("If this is off, the weapon won't display in alternate weapon slots (some bows and very large weapons).")]
	public bool DisplayWhenAlternate = true;

	[Tooltip("If set, the weapon will use the back scabbard when unequipped, regardless of its Weapon Type.")]
	public bool BackScabbardOverride;

	public bool IsImplement;

	public WeaponSpecializationData.WeaponType WeaponType;

	[Tooltip("If set, any WeaponFocus and WeaponSpecialization can apply to this weapon, regardless of the Weapon Type.")]
	public bool UniversalType;

	public Stance AnimationStance = Stance.OneHanded;

	public override void Equip(GameObject character, EquipmentSlot slot)
	{
		base.Equip(character, slot);
		AnimationController component = character.GetComponent<AnimationController>();
		if ((bool)component)
		{
			if (slot == EquipmentSlot.PrimaryWeapon)
			{
				component.Stance = (int)AnimationStance;
			}
			if (slot != EquipmentSlot.PrimaryWeapon2 && slot != EquipmentSlot.SecondaryWeapon2)
			{
				Animator component2 = GetComponent<Animator>();
				if ((bool)component2 && !component.SyncList.Contains(component2))
				{
					component2.logWarnings = false;
					component.SyncList.Add(component2);
				}
			}
		}
		AttackBase component3 = GetComponent<AttackBase>();
		if (component3 != null)
		{
			component3.Owner = character;
		}
	}

	public override Equippable UnEquip(GameObject character, EquipmentSlot slot)
	{
		AnimationController component = character.GetComponent<AnimationController>();
		if ((bool)component)
		{
			if (slot == EquipmentSlot.PrimaryWeapon)
			{
				component.Stance = 0;
			}
			if (slot != EquipmentSlot.PrimaryWeapon2 && slot != EquipmentSlot.SecondaryWeapon2)
			{
				Animator component2 = GetComponent<Animator>();
				if ((bool)component2)
				{
					component.SyncList.Remove(component2);
				}
			}
		}
		return base.UnEquip(character, slot);
	}

	public override void ApplyLaunchEffects(GameObject parent)
	{
		CharacterStats component = parent.GetComponent<CharacterStats>();
		if (component != null)
		{
			StatusEffectParams[] statusEffectsOnLaunch = StatusEffectsOnLaunch;
			foreach (StatusEffectParams statusEffectParams in statusEffectsOnLaunch)
			{
				if (statusEffectParams != null)
				{
					StatusEffect statusEffect = StatusEffect.Create(parent, this, statusEffectParams, GenericAbility.AbilityType.WeaponOrShield, null, deleteOnClear: true);
					if (StatusEffectsExtraObjectIsSummoner && base.SummoningEffect != null)
					{
						statusEffect.ExtraObject = base.SummoningEffect.Owner;
					}
					component.ApplyStatusEffectImmediate(statusEffect);
				}
			}
		}
		base.ApplyLaunchEffects(parent);
	}

	public override float FindLaunchAccuracyBonus(AttackBase attack)
	{
		float num = base.FindLaunchAccuracyBonus(attack);
		StatusEffectParams[] statusEffectsOnLaunch = StatusEffectsOnLaunch;
		foreach (StatusEffectParams statusEffectParams in statusEffectsOnLaunch)
		{
			if (statusEffectParams != null)
			{
				num += statusEffectParams.EstimateAccuracyBonusForUi(attack);
			}
		}
		return num;
	}

	public override void AdjustDamageForUi(GameObject character, DamageInfo damage)
	{
		StatusEffectParams[] statusEffectsOnLaunch = StatusEffectsOnLaunch;
		for (int i = 0; i < statusEffectsOnLaunch.Length; i++)
		{
			statusEffectsOnLaunch[i]?.AdjustDamageForUi(character, damage);
		}
		base.AdjustDamageForUi(character, damage);
	}

	public void PlayHitSound()
	{
		if ((bool)base.EquippedOwner)
		{
			AudioBank component = base.EquippedOwner.GetComponent<AudioBank>();
			ClipBankSet weaponHitSoundSet = GlobalAudioPlayer.Instance.GetWeaponHitSoundSet(WeaponType);
			if ((bool)component && weaponHitSoundSet != null)
			{
				component.PlayFrom(weaponHitSoundSet);
			}
		}
	}

	public bool PlayMissSound()
	{
		if ((bool)base.EquippedOwner)
		{
			AudioBank component = base.EquippedOwner.GetComponent<AudioBank>();
			ClipBankSet weaponMissSoundSet = GlobalAudioPlayer.Instance.GetWeaponMissSoundSet(WeaponType);
			if ((bool)component && weaponMissSoundSet != null)
			{
				component.PlayFrom(weaponMissSoundSet);
				return true;
			}
		}
		return false;
	}

	public bool PlayMinDamageSound()
	{
		if ((bool)base.EquippedOwner)
		{
			AudioBank component = base.EquippedOwner.GetComponent<AudioBank>();
			ClipBankSet weaponMinDamageSoundSet = GlobalAudioPlayer.Instance.GetWeaponMinDamageSoundSet(WeaponType);
			if ((bool)component && weaponMinDamageSoundSet != null)
			{
				component.PlayFrom(weaponMinDamageSoundSet);
				return true;
			}
		}
		return false;
	}
}
