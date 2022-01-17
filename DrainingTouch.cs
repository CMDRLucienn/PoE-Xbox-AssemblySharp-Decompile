using UnityEngine;

public class DrainingTouch : GenericSpell
{
	public float DrainPercentage = 0.25f;

	protected override void HandleStatsOnPostDamageDealt(GameObject source, CombatEventArgs args)
	{
		if (!args.Damage.IsMiss && m_ownerHealth != null)
		{
			m_ownerHealth.AddHealth(args.Damage.DamageAmount * DrainPercentage);
		}
	}

	protected override void ActivateStatusEffects()
	{
	}
}
