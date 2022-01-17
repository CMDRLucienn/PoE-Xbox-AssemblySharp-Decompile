using System.Collections.Generic;
using UnityEngine;

namespace AI.Plan;

public class WaitForClearPath : GameAIState
{
	public Mover Blocker;

	public float BlockerDistance = 0.5f;

	public List<Mover> Obstacles = new List<Mover>();

	private List<Vector3> PreviousPositions = new List<Vector3>();

	private float DefaultTimer;

	public override bool UseQueuedTarget => true;

	public override void Reset()
	{
		base.Reset();
		Blocker = null;
		BlockerDistance = 0.5f;
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
		if (DefaultTimer >= 0f)
		{
			DefaultTimer -= Time.deltaTime;
		}
		m_ai.UpdateEngagement(base.Owner, AIController.GetPrimaryAttack(base.Owner));
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
			GameObject currentTarget = m_ai.CurrentTarget;
			if (currentTarget != null)
			{
				AttackBase currentAttack = m_ai.CurrentAttack;
				if (currentAttack != null)
				{
					float totalAttackDistance = currentAttack.TotalAttackDistance;
					Mover mover2 = m_ai.Mover;
					Mover component = currentTarget.GetComponent<Mover>();
					totalAttackDistance += mover2.Radius;
					if (component != null)
					{
						totalAttackDistance += component.Radius;
					}
					if (GameUtilities.V3SqrDistance2D(currentTarget.transform.position, mover2.transform.position) <= totalAttackDistance * totalAttackDistance)
					{
						mover2.ClearForcedTurnDirection();
						base.Manager.PopCurrentState();
						return;
					}
				}
				m_ai.FaceTarget(AIController.GetPrimaryAttack(base.Owner));
			}
			float num3 = BlockerDistance * BlockerDistance;
			for (int i = 0; i < Obstacles.Count; i++)
			{
				if (Obstacles[i] == null || !m_ai.IsTargetable(Obstacles[i].gameObject) || PreviousPositions.Count <= i || GameUtilities.V3SqrDistance2D(Obstacles[i].transform.position, PreviousPositions[i]) >= num3)
				{
					Mover mover3 = m_ai.Mover;
					mover3.ClearForcedTurnDirection();
					mover3.IgnoreAttackBlocking = true;
					base.Manager.PopCurrentState();
					return;
				}
			}
		}
		TargetScanner targetScanner = m_ai.GetTargetScanner();
		if (targetScanner == null)
		{
			return;
		}
		if (m_ai.EngagedEnemies.Count > 0)
		{
			foreach (GameObject engagedEnemy in m_ai.EngagedEnemies)
			{
				Health component2 = engagedEnemy.GetComponent<Health>();
				if (component2 != null && !component2.Dead && !component2.Unconscious)
				{
					m_ai.StateManager.PopAllStates();
					ApproachTarget approachTarget = PushState<ApproachTarget>();
					approachTarget.TargetScanner = targetScanner;
					approachTarget.Target = engagedEnemy;
					return;
				}
			}
		}
		if (m_ai.EngagedBy.Count <= 0)
		{
			return;
		}
		foreach (GameObject engagedEnemy2 in m_ai.EngagedEnemies)
		{
			Health component3 = engagedEnemy2.GetComponent<Health>();
			if (component3 != null && !component3.Dead && !component3.Unconscious)
			{
				m_ai.StateManager.PopAllStates();
				ApproachTarget approachTarget2 = PushState<ApproachTarget>();
				approachTarget2.TargetScanner = targetScanner;
				approachTarget2.Target = engagedEnemy2;
				break;
			}
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
