using UnityEngine;

[ClassTooltip("When an enemy disengages the owner, launches the attached attack at that enemy.")]
public class CripplingGuard : GenericAbility
{
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
		if (args.Victim != null && !args.Damage.IsGraze && !args.Damage.IsMiss && args.Damage.Attack.IsDisengagementAttack && (bool)m_attackBase)
		{
			m_attackBase.SkipAnimation = true;
			m_attackBase.LaunchingDirectlyToImpact = false;
			m_attackBase.Launch(args.Victim, this);
		}
	}

	public override AttackBase.FormattableTarget GetAbilityTarget()
	{
		return AttackBase.GENERAL_TARGET;
	}
}
