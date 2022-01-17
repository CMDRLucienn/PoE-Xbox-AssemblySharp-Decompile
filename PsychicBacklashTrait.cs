using UnityEngine;

public class PsychicBacklashTrait : GenericAbility
{
	protected override void HandleStatsOnPostDamageApplied(GameObject source, CombatEventArgs args)
	{
		if (args.Damage.DefendedBy == CharacterStats.DefenseType.Will && args.Attacker != null)
		{
			AttackMelee component = GetComponent<AttackMelee>();
			if (component != null && component != args.Damage.Attack)
			{
				component.SkipAnimation = true;
				component.Launch(args.Attacker, this);
			}
		}
	}

	protected override void Init()
	{
		if (!m_initialized)
		{
			base.Init();
			m_permanent = true;
		}
	}
}
