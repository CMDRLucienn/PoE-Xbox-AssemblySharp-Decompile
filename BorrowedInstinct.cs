using UnityEngine;

public class BorrowedInstinct : GenericCipherAbility
{
	protected override void HandleStatsOnPostDamageDealt(GameObject source, CombatEventArgs args)
	{
		if (args.Attacker != null && !args.Damage.IsMiss)
		{
			ApplyStatusEffectsOnHit(args.Attacker);
		}
	}

	public void ApplyStatusEffectsOnHit(GameObject target)
	{
		CharacterStats component = target.GetComponent<CharacterStats>();
		foreach (StatusEffect effect in m_effects)
		{
			component.ApplyStatusEffectImmediate(effect);
		}
		AttackBase.PostAttackMessages(target, Owner, new DamageInfo(target, CharacterStats.DefenseType.None, this), m_effects, primaryAttack: true);
	}

	protected override void ActivateStatusEffects()
	{
	}
}
