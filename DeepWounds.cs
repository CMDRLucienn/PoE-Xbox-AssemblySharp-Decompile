using UnityEngine;

public class DeepWounds : GenericAbility
{
	protected override void HandleStatsOnPostDamageDealt(GameObject source, CombatEventArgs args)
	{
		if (!args.Damage.IsMiss && !(args.Victim == null) && !(Owner == args.Victim) && (args.Damage.Damage.Type == DamagePacket.DamageType.Slash || args.Damage.Damage.Type == DamagePacket.DamageType.Pierce || args.Damage.Damage.Type == DamagePacket.DamageType.Crush))
		{
			ApplyStatusEffectsOnHit(args.Victim);
		}
	}

	public void ApplyStatusEffectsOnHit(GameObject enemy)
	{
		CharacterStats component = enemy.GetComponent<CharacterStats>();
		foreach (StatusEffect effect in m_effects)
		{
			effect.Params.DontHideFromLog = true;
			component.ApplyStatusEffectImmediate(effect);
		}
		DamageInfo damageInfo = new DamageInfo(enemy, CharacterStats.DefenseType.None, this);
		damageInfo.OtherOwner = Owner;
		AttackBase.PostAttackMessages(enemy, Owner, damageInfo, m_effects, primaryAttack: true);
	}

	protected override void ActivateStatusEffects()
	{
	}

	public override AttackBase.FormattableTarget GetAbilityTarget()
	{
		return AttackBase.GENERAL_TARGET;
	}
}
