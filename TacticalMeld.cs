using UnityEngine;

public class TacticalMeld : GenericCipherAbility
{
	protected override void HandleStatsOnPostDamageDealt(GameObject source, CombatEventArgs args)
	{
		if (args.Attacker != null && args.Victim != null && !args.Damage.IsMiss)
		{
			ApplyStatusEffectsOnHit(args.Attacker, args.Victim);
		}
	}

	public void ApplyStatusEffectsOnHit(GameObject target, GameObject meldTarget)
	{
		CharacterStats component = target.GetComponent<CharacterStats>();
		foreach (StatusEffect effect in m_effects)
		{
			effect.ExtraObject = meldTarget;
			component.ApplyStatusEffectImmediate(effect);
		}
		AttackBase.PostAttackMessages(target, Owner, new DamageInfo(target, CharacterStats.DefenseType.None, this), m_effects, primaryAttack: true);
	}

	protected override void ActivateStatusEffects()
	{
	}
}
