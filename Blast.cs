using UnityEngine;

public class Blast : GenericAbility
{
	public GameObject OnBlastHitVisualEffect;

	protected override void HandleStatsOnPostDamageDealt(GameObject source, CombatEventArgs args)
	{
		BlastArea(args.Damage, args.Attacker, args.Victim);
	}

	protected override void Init()
	{
		if (!m_initialized)
		{
			base.Init();
			m_permanent = true;
		}
	}

	public bool BlastApplies(AttackBase attack)
	{
		Weapon weapon = (attack ? attack.GetComponent<Weapon>() : null);
		if ((bool)weapon)
		{
			return weapon.IsImplement;
		}
		return false;
	}

	public void BlastArea(DamageInfo damage, GameObject attacker, GameObject victim)
	{
		if (!damage.IsMiss && !(attacker == null) && !(victim == null) && BlastApplies(damage.Attack))
		{
			AttackAOE component = GetComponent<AttackAOE>();
			if (component != null)
			{
				Transform parent = AttackBase.GetTransform(victim, damage.Attack.OnHitAttach);
				GameUtilities.LaunchEffect(OnBlastHitVisualEffect, 1f, parent, this);
				component.OnImpactShared(null, victim.transform.position, victim);
			}
		}
	}
}
