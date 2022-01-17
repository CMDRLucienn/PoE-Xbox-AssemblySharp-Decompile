using System.Collections.Generic;
using AI.Achievement;
using UnityEngine;

namespace AI.Player;

public class WaitForClearPath : PlayerState
{
	public Mover Blocker;

	public float BlockerDistance = 0.5f;

	public List<Mover> Obstacles = new List<Mover>();

	private List<Vector3> PreviousPositions = new List<Vector3>();

	public float WaitTimer;

	private float DefaultTimer;

	public override bool UseQueuedTarget => true;

	public override void Reset()
	{
		base.Reset();
		Blocker = null;
		BlockerDistance = 0.5f;
		WaitTimer = 0f;
		DefaultTimer = 0.35f;
		Obstacles.Clear();
		PreviousPositions.Clear();
	}

	public override void OnEnter()
	{
		base.OnEnter();
		StopMover();
		if (m_animation != null)
		{
			m_animation.DesiredAction.m_actionType = AnimationController.ActionType.None;
		}
		PreviousPositions.Clear();
		for (int num = Obstacles.Count - 1; num >= 0; num--)
		{
			if (Obstacles[num] == null)
			{
				Obstacles.RemoveAt(num);
			}
		}
		if (Obstacles.Count == 0)
		{
			base.Manager.PopCurrentState();
			return;
		}
		foreach (Mover obstacle in Obstacles)
		{
			PreviousPositions.Add(obstacle.transform.position);
		}
	}

	public override void Update()
	{
		base.Update();
		if (WaitTimer >= 0f)
		{
			WaitTimer -= Time.deltaTime;
		}
		if (DefaultTimer >= 0f)
		{
			DefaultTimer -= Time.deltaTime;
		}
		m_ai.UpdateEngagement(base.Owner, m_partyMemberAI.GetPrimaryAttack());
		if (!(WaitTimer < 0f))
		{
			return;
		}
		if (Blocker != null)
		{
			if (!Blocker.IsPathingObstacle() || !Blocker.IsMoving())
			{
				base.Manager.PopCurrentState();
				return;
			}
			float num = GameUtilities.V3SqrDistance2D(m_owner.transform.position, Blocker.gameObject.transform.position);
			Mover mover = m_ai.Mover;
			float num2 = mover.Radius + Blocker.Radius + BlockerDistance;
			if (num >= num2 * num2)
			{
				base.Manager.PopCurrentState();
				return;
			}
			if (DefaultTimer < 0f && Vector2.Angle(GameUtilities.V3Subtract2D(Blocker.gameObject.transform.position, m_owner.transform.position), mover.MovementDirection) > 45f)
			{
				base.Manager.PopCurrentState();
				return;
			}
		}
		else
		{
			if (Obstacles.Count <= 0)
			{
				base.Manager.PopCurrentState();
				return;
			}
			m_ai.FaceTarget(m_partyMemberAI.GetPrimaryAttack());
			float num3 = BlockerDistance * BlockerDistance;
			for (int i = 0; i < Obstacles.Count; i++)
			{
				if (!(Obstacles[i] == null) && Vector3.SqrMagnitude(Obstacles[i].transform.position - PreviousPositions[i]) >= num3)
				{
					Mover mover2 = m_ai.Mover;
					mover2.ClearAllTurnDirections();
					mover2.ClearDesiredHeading();
					mover2.IgnoreAttackBlocking = true;
					base.Manager.PopCurrentState();
					return;
				}
			}
		}
		PartyMemberAI partyMemberAI = m_ai as PartyMemberAI;
		if (partyMemberAI != null && partyMemberAI.Aggression != AIController.AggressionType.Passive && partyMemberAI.StateManager.QueuedState is PathToPosition && partyMemberAI.StateManager.GetQueuedState(2) is Attack attack && attack.IsAutoAttack && (partyMemberAI.EngagedBy.Count > 0 || partyMemberAI.EngagedEnemies.Count > 0 || !attack.CanAttackTarget()))
		{
			partyMemberAI.StateManager.PopCurrentState();
			partyMemberAI.StateManager.PopCurrentState();
			partyMemberAI.StateManager.PopCurrentState();
		}
	}

	public override string GetDebugText()
	{
		if (Blocker != null)
		{
			return ": Blocker:" + Blocker.ToString();
		}
		return string.Empty;
	}

	public override bool CanBeNudgedBy(Mover pather)
	{
		return false;
	}

	public override bool IsPathBlocked()
	{
		return Blocker == null;
	}

	public override bool TurnWhilePaused()
	{
		return false;
	}
}
