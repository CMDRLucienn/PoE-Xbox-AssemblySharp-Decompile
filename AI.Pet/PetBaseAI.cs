using UnityEngine;

namespace AI.Pet;

public class PetBaseAI : AIState
{
	protected const float MAX_DIST = 6f;

	protected const float MAX_DIST_SQ = 36f;

	protected GameObject m_master;

	protected float m_distToMasterSq;

	public override void Reset()
	{
		base.Reset();
		m_master = null;
		m_distToMasterSq = 0f;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_master = m_ai.Summoner;
		if (m_master == null)
		{
			m_master = GameState.s_playerCharacter.gameObject;
		}
		UpdateDistanceToMaster();
	}

	public override void Update()
	{
		base.Update();
		if (m_master == null)
		{
			m_master = m_ai.Summoner;
			if (m_master == null && GameState.s_playerCharacter != null)
			{
				m_master = GameState.s_playerCharacter.gameObject;
			}
		}
		else
		{
			UpdateDistanceToMaster();
		}
	}

	private void UpdateDistanceToMaster()
	{
		if (m_master != null)
		{
			m_distToMasterSq = (m_master.transform.position - m_owner.transform.position).sqrMagnitude;
		}
	}

	public override bool IsPathingObstacle()
	{
		return false;
	}

	public override bool CanBeNudgedBy(Mover pather)
	{
		return true;
	}

	public override bool PerformsSoftSteering()
	{
		return true;
	}
}
