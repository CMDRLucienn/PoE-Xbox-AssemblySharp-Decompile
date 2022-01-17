using UnityEngine;

namespace AI.Player;

public class WaitForEngagementToEnd : Wait
{
	public override void Update()
	{
		if (GameState.IsLoading)
		{
			return;
		}
		base.Update();
		m_stats.IdleTimer += Time.deltaTime;
		if (m_partyMemberAI.GetAutoAttackAggression() != AIController.AggressionType.Passive || !GameState.InCombat)
		{
			m_partyMemberAI.StateManager.PopCurrentState();
			return;
		}
		m_partyMemberAI.ReloadIfNecessary();
		m_fidgetController.Update();
		m_ai.UpdateEngagement(base.Owner, m_partyMemberAI.GetPrimaryAttack());
		if (m_partyMemberAI.EngagedBy.Count <= 0)
		{
			m_partyMemberAI.StateManager.PopCurrentState();
		}
	}

	public override bool CanBeNudgedBy(Mover pather)
	{
		return false;
	}

	public override bool IsIdling()
	{
		return false;
	}
}
