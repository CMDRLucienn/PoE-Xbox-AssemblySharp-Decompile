using UnityEngine;

public class DefensiveShooting : GenericAbility
{
	protected override void Init()
	{
		if (!m_initialized)
		{
			base.Init();
			m_permanent = true;
		}
	}

	protected override void HandleStatsOnPreDamageDealt(GameObject source, CombatEventArgs args)
	{
		if (CanDefensivelyShootTarget(args.Damage.Attack, args.Victim))
		{
			ApplyStatusEffectsNow();
		}
	}

	protected override void HandleStatsOnPostDamageDealt(GameObject source, CombatEventArgs args)
	{
		if (CanDefensivelyShootTarget(args.Damage.Attack, args.Victim) && args.Damage.IsCriticalHit)
		{
			_ = Owner.GetComponent<AIController>() != null;
		}
	}

	private bool CanDefensivelyShootTarget(AttackBase attack, GameObject target)
	{
		if (target == null)
		{
			return false;
		}
		AIController component = target.GetComponent<AIController>();
		if (component == null)
		{
			return false;
		}
		if (!component.EngagedEnemies.Contains(Owner))
		{
			return false;
		}
		if (!(attack is AttackRanged))
		{
			return false;
		}
		Equipment component2 = Owner.GetComponent<Equipment>();
		if (component2 == null)
		{
			return false;
		}
		if (attack != component2.PrimaryAttack && attack != component2.SecondaryAttack)
		{
			return false;
		}
		return true;
	}

	public void ApplyStatusEffectsNow()
	{
		CharacterStats component = Owner.GetComponent<CharacterStats>();
		foreach (StatusEffect effect in m_effects)
		{
			component.ApplyStatusEffectImmediate(effect);
		}
		AttackBase.PostAttackMessages(Owner, Owner, new DamageInfo(Owner, CharacterStats.DefenseType.None, this), m_effects, primaryAttack: true);
	}

	protected override void ActivateStatusEffects()
	{
	}
}
