using UnityEngine;

public class GenericSpellWithPositionSwap : GenericSpell
{
	protected override void HandleStatsOnBeamHits(GameObject source, CombatEventArgs args)
	{
		base.HandleStatsOnBeamHits(source, args);
		if (args.Victim != null)
		{
			AIController.BreakAllEngagements(args.Victim);
			AIController.BreakAllEngagements(m_owner);
			Vector3 position = m_owner.transform.position;
			m_owner.transform.position = args.Victim.transform.position;
			args.Victim.transform.position = position;
		}
	}
}
