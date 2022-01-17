using UnityEngine;

[ClassTooltip("When the owner attacks a target, launches an extra attack based on his weapon's Weapon Type, then applies the ability's status effects to the owner with the specified (unadjusted) duration based on Weapon Type.")]
public class PowderBurns : GenericAbility
{
	public AttackBase AttackPistol;

	public AttackBase AttackArquebus;

	public AttackBase AttackBlunderbuss;

	public float PistolEffectDuration;

	public float ArquebusEffectDuration;

	public float BlunderbussEffectDuration;

	protected override void HandleStatsOnAttackHitFrame(GameObject source, CombatEventArgs args)
	{
		AttackBase burnForAttack = GetBurnForAttack(args.Damage.Attack);
		if (burnForAttack != null)
		{
			AttackBase attackBase = Object.Instantiate(burnForAttack);
			attackBase.DestroyAfterImpact = true;
			attackBase.Owner = Owner;
			attackBase.transform.parent = Owner.transform;
			attackBase.SkipAnimation = true;
			attackBase.DoNotCleanOneHitEffects = true;
			attackBase.Launch(source, this);
		}
		base.HandleStatsOnAttackHitFrame(source, args);
	}

	protected override void ActivateStatusEffects()
	{
	}

	protected override void HandleStatsOnPostDamageDealt(GameObject source, CombatEventArgs args)
	{
		Weapon weapon = (args.Damage.Attack ? args.Damage.Attack.GetComponent<Weapon>() : null);
		if ((bool)weapon)
		{
			float num = weapon.WeaponType switch
			{
				WeaponSpecializationData.WeaponType.Pistol => PistolEffectDuration, 
				WeaponSpecializationData.WeaponType.Arquebus => ArquebusEffectDuration, 
				WeaponSpecializationData.WeaponType.Blunderbuss => BlunderbussEffectDuration, 
				_ => 0f, 
			};
			if (num > 0f)
			{
				foreach (StatusEffect effect in m_effects)
				{
					effect.Params.Duration = 0f;
					effect.UnadjustedDurationAdd = num;
					m_ownerStats.ApplyStatusEffectImmediate(effect);
				}
			}
		}
		base.HandleStatsOnPostDamageDealt(source, args);
	}

	public bool PowderBurnsApplies(AttackBase attack)
	{
		if (!attack)
		{
			return false;
		}
		Weapon component = attack.GetComponent<Weapon>();
		if (!component)
		{
			return false;
		}
		if (component.WeaponType != WeaponSpecializationData.WeaponType.Pistol && component.WeaponType != WeaponSpecializationData.WeaponType.Arquebus)
		{
			return component.WeaponType == WeaponSpecializationData.WeaponType.Blunderbuss;
		}
		return true;
	}

	public AttackBase GetBurnForAttack(AttackBase attack)
	{
		if (!attack)
		{
			return null;
		}
		Weapon component = attack.GetComponent<Weapon>();
		if (!component)
		{
			return null;
		}
		return component.WeaponType switch
		{
			WeaponSpecializationData.WeaponType.Pistol => AttackPistol, 
			WeaponSpecializationData.WeaponType.Arquebus => AttackArquebus, 
			WeaponSpecializationData.WeaponType.Blunderbuss => AttackBlunderbuss, 
			_ => null, 
		};
	}
}
