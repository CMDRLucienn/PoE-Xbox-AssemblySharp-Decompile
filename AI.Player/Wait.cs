using UnityEngine;

namespace AI.Player;

public class Wait : PlayerState
{
	protected FidgetController m_fidgetController = new FidgetController();

	public override void Reset()
	{
		base.Reset();
		m_fidgetController.Reset();
	}

	public override void OnEnter()
	{
		base.OnEnter();
		StopMover();
		InitFidgetController(m_fidgetController, null);
		if (m_animation != null)
		{
			m_animation.DesiredAction.m_actionType = AnimationController.ActionType.None;
		}
		m_ai.CancelAllEngagements();
		m_partyMemberAI.ReloadIfNecessary();
	}

	public override void Update()
	{
		if (GameState.IsLoading)
		{
			return;
		}
		base.Update();
		m_stats.IdleTimer += Time.deltaTime;
		if (GameState.InCombat && !Stealth.IsInStealthMode(base.Owner))
		{
			AIController.AggressionType autoAttackAggression = m_partyMemberAI.GetAutoAttackAggression();
			if (autoAttackAggression != AIController.AggressionType.Passive && autoAttackAggression != 0 && m_partyMemberAI.AutoPickNearbyEnemy(null))
			{
				return;
			}
		}
		m_partyMemberAI.ReloadIfNecessary();
		m_fidgetController.Update();
		m_ai.UpdateEngagement(base.Owner, m_partyMemberAI.GetPrimaryAttack());
	}

	public override bool CanBeNudgedBy(Mover pather)
	{
		if (m_ai.EngagedEnemies.Count > 0)
		{
			return false;
		}
		Faction component = m_owner.GetComponent<Faction>();
		if (component != null)
		{
			return !component.IsHostile(pather.gameObject);
		}
		return false;
	}

	public override bool IsIdling()
	{
		return true;
	}
}
