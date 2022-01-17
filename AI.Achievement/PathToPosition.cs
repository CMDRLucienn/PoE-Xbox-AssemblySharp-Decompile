using System;
using AI.Plan;
using UnityEngine;
using UnityEngine.AI;

namespace AI.Achievement;

public class PathToPosition : GameAIState
{
	public class Params
	{
		public Vector3 Destination;

		public GameObject Target;

		public float Range;

		public bool DesiresMaxRange;

		public AnimationController.MovementType MovementType;

		public bool OpenBlockingDoors = true;

		public bool LineOfSight;

		public bool StopOnLOS;

		public bool GetAsCloseAsPossible;

		public bool PopOnEnterIfTargetInvalid;

		public bool IgnoreObstaclesWithinRange;

		public TargetScanner TargetScanner;

		public Params()
		{
			Reset();
		}

		public void Reset()
		{
			Destination = Vector3.zero;
			Target = null;
			Range = 0f;
			DesiresMaxRange = false;
			MovementType = AnimationController.MovementType.None;
			OpenBlockingDoors = true;
			LineOfSight = false;
			StopOnLOS = false;
			GetAsCloseAsPossible = false;
			PopOnEnterIfTargetInvalid = false;
			IgnoreObstaclesWithinRange = false;
			TargetScanner = null;
		}
	}

	protected Params m_params = new Params();

	private PathingController m_pathingController = new PathingController();

	protected float m_repathTimer;

	protected int m_retriesLeft;

	private bool m_hasBeenUpdated;

	private float m_scanTimer;

	private AIState m_parentState;

	private bool m_frozen;

	public Params Parameters
	{
		get
		{
			return m_params;
		}
		set
		{
			m_params = value;
		}
	}

	public AnimationController.MovementType MovementType
	{
		get
		{
			return m_params.MovementType;
		}
		set
		{
			m_params.MovementType = value;
			Mover mover = m_pathingController.Mover;
			if (!(mover == null))
			{
				if (m_params.MovementType == AnimationController.MovementType.Walk)
				{
					mover.UseWalkSpeed();
					m_animation.Walk = true;
				}
				else
				{
					mover.UseRunSpeed();
					m_animation.Walk = false;
				}
			}
		}
	}

	public override bool InCombat => m_ai.StateManager.QueuedState?.InCombat ?? false;

	public override bool UseQueuedTarget => true;

	public override AIState ParentState
	{
		get
		{
			return m_parentState;
		}
		set
		{
			m_parentState = value;
		}
	}

	public override void Reset()
	{
		base.Reset();
		m_repathTimer = 0f;
		m_retriesLeft = 0;
		m_hasBeenUpdated = false;
		m_scanTimer = 0f;
		m_params.Reset();
		m_parentState = null;
		m_frozen = false;
		m_pathingController.Reset();
	}

	public override void OnEnter()
	{
		base.OnEnter();
		if (m_owner == null)
		{
			Debug.LogError("PathToPosition has a null owner!");
			base.Manager.PopCurrentState();
			return;
		}
		m_hasBeenUpdated = false;
		Mover mover = m_ai.Mover;
		if (mover != null)
		{
			mover.ClearForcedTurnDirection();
			mover.ReachedGoal = false;
			mover.IgnoreAttackBlocking = false;
			mover.IgnoreObstaclesWithinRange = m_params.IgnoreObstaclesWithinRange;
			mover.enabled = true;
			m_frozen = mover.Frozen;
			if (mover.AIController is PartyMemberAI)
			{
				m_params.OpenBlockingDoors = false;
			}
		}
		if (m_params.PopOnEnterIfTargetInvalid && (m_params.Target == null || !m_ai.IsTargetable(m_params.Target)))
		{
			m_ai.StateManager.PopCurrentState();
			return;
		}
		m_pathingController.Init(this, m_params, startPathing: true);
		m_pathingController.Mover.OnMovementBlocked += m_agent_OnMovementBlocked;
		if (m_pathingController.GetDestination().sqrMagnitude < float.Epsilon)
		{
			m_ai.StateManager.PopCurrentState();
		}
		m_scanTimer = UnityEngine.Random.Range(AttackData.Instance.MinTargetReevaluationTime, AttackData.Instance.MaxTargetReevaluationTime);
	}

	private void m_agent_OnMovementBlocked(object sender, EventArgs e)
	{
		base.Manager.PopCurrentState();
	}

	public override void OnExit()
	{
		base.OnExit();
		m_pathingController.Stop();
		if (m_pathingController.Mover != null)
		{
			m_pathingController.Mover.OnMovementBlocked -= m_agent_OnMovementBlocked;
		}
		if (m_ai.Mover != null)
		{
			m_ai.Mover.IgnoreObstaclesWithinRange = false;
		}
	}

	public override void OnAbort()
	{
		base.OnAbort();
		m_pathingController.Stop();
		if (m_pathingController.Mover != null)
		{
			m_pathingController.Mover.OnMovementBlocked -= m_agent_OnMovementBlocked;
		}
	}

	public override void Update()
	{
		base.Update();
		if (m_params.Target == null && m_params.Destination.sqrMagnitude < float.Epsilon)
		{
			base.Manager.PopCurrentState();
			return;
		}
		if (m_ai.BeingKited())
		{
			m_ai.StateManager.PopCurrentState();
			m_ai.StopKiting();
			return;
		}
		if (m_params.TargetScanner != null)
		{
			AIState currentState = base.Manager.CurrentState;
			if (m_params.Target != null)
			{
				m_scanTimer -= Time.deltaTime;
				if (m_scanTimer <= 0f)
				{
					m_scanTimer = UnityEngine.Random.Range(AttackData.Instance.MinTargetReevaluationTime, AttackData.Instance.MaxTargetReevaluationTime);
					AIState aIState = base.Manager.FindState(typeof(ApproachTarget));
					if (m_params.TargetScanner.ScanForTarget(m_owner, m_ai, -1f, ignoreIfCurrentTarget: true))
					{
						base.Manager.PopState(currentState);
						if (aIState != null)
						{
							base.Manager.PopState(aIState);
						}
						return;
					}
					if (base.Manager.CurrentState != this)
					{
						return;
					}
				}
			}
			else if (m_params.TargetScanner.ScanForTarget(m_owner, m_ai, -1f, ignoreIfCurrentTarget: true))
			{
				base.Manager.PopState(currentState);
				return;
			}
		}
		m_hasBeenUpdated = true;
		if (m_frozen)
		{
			if (!m_ai.Mover.Frozen)
			{
				m_pathingController.Init(this, m_params, startPathing: true);
				m_frozen = false;
			}
		}
		else
		{
			m_frozen = m_ai.Mover.Frozen;
		}
		m_pathingController.Update();
		if (m_ai is PartyMemberAI)
		{
			m_pathingController.UpdateStealth();
		}
		m_ai.UpdateEngagement(base.Owner, AIController.GetPrimaryAttack(base.Owner));
		if (m_pathingController.ReachedDestination())
		{
			base.Manager.PopCurrentState();
		}
		m_pathingController.UpdatePreviousPosition();
	}

	public override void OnCancel()
	{
		if (m_pathingController.Mover != null)
		{
			m_pathingController.Mover.Stop();
		}
	}

	public void SetDestination(Vector3 position)
	{
		Vector3 vector = position;
		if (NavMesh.SamplePosition(vector, out var hit, 2f, int.MaxValue))
		{
			vector = hit.position;
		}
		Mover mover = m_pathingController.Mover;
		if (mover != null)
		{
			vector.y = mover.Goal.y;
			if (GameUtilities.V3SqrDistance2D(mover.Goal, vector) > m_params.Range * m_params.Range + float.Epsilon)
			{
				mover.PathToDestination(position, 0f);
				m_retriesLeft = 1;
				if (m_params.MovementType == AnimationController.MovementType.Walk)
				{
					mover.UseWalkSpeed();
					m_animation.Walk = true;
				}
				else
				{
					mover.UseRunSpeed();
					m_animation.Walk = false;
				}
			}
		}
		m_params.Destination = vector;
	}

	public override bool IsMoving()
	{
		return m_ai.Mover.Speed > float.Epsilon;
	}

	public override bool AllowBlockedMovement()
	{
		return m_hasBeenUpdated;
	}

	public override bool TurnWhilePaused()
	{
		return false;
	}
}
