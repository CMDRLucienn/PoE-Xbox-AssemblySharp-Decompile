using System.Linq;
using UnityEngine;

public class EnervatingBlows : GenericAbility
{
	public CharacterStats.DefenseType EnervatingBlowsDefense = CharacterStats.DefenseType.Fortitude;

	protected override void Init()
	{
		if (!m_initialized)
		{
			base.Init();
			m_permanent = true;
		}
	}

	protected override void HandleStatsOnPostDamageDealt(GameObject source, CombatEventArgs args)
	{
		if (args.Damage.IsCriticalHit && args.Victim != null && args.Damage.Attack is AttackMelee && m_ownerStats != null)
		{
			DamageInfo damageInfo = m_ownerStats.ComputeSecondaryAttack(args.Damage.Attack, args.Victim, EnervatingBlowsDefense);
			if (damageInfo.HitType != 0)
			{
				ApplyAfflictionsOnMeleeCrit(args.Victim, damageInfo);
			}
			AttackBase.PostAttackMessagesSecondaryEffect(args.Victim, damageInfo, m_effects.Where((StatusEffect eff) => eff.AfflictionOrigin != null));
		}
	}

	public void ApplyAfflictionsOnMeleeCrit(GameObject enemy, DamageInfo hitInfo)
	{
		CharacterStats component = enemy.GetComponent<CharacterStats>();
		if (!(component != null))
		{
			return;
		}
		foreach (StatusEffect effect in m_effects)
		{
			if (effect.AfflictionOrigin != null)
			{
				effect.SetHitInfo(hitInfo);
				component.ApplyStatusEffectImmediate(effect);
			}
		}
	}

	protected override void ActivateStatusEffects()
	{
	}

	public override AttackBase.FormattableTarget GetAbilityTarget()
	{
		return AttackBase.GENERAL_TARGET;
	}
}
