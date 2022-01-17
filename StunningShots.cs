using UnityEngine;

[ClassTooltip("When the owner Hits or Crits a target with a ranged weapon while the owner's animal companion is engaging the target, launches the attached attack at the target.")]
public class StunningShots : GenericAbility
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
		if (args.Victim == null || args.Damage.IsMiss || args.Damage.IsGraze || args.Damage.Attack == null)
		{
			return;
		}
		Equipment component = Owner.GetComponent<Equipment>();
		if (component == null || (args.Damage.Attack != component.PrimaryAttack && args.Damage.Attack != component.SecondaryAttack))
		{
			return;
		}
		GameObject gameObject = GameUtilities.FindAnimalCompanion(Owner);
		if (gameObject == null)
		{
			return;
		}
		AIController component2 = gameObject.GetComponent<AIController>();
		if (!(component2 == null) && component2.EngagedEnemies.Contains(args.Victim))
		{
			AttackBase component3 = GetComponent<AttackBase>();
			if (!(component3 == null))
			{
				component3.SkipAnimation = true;
				component3.Launch(args.Victim, this);
			}
		}
	}
}
