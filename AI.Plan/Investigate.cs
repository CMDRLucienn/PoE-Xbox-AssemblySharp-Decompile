using AI.Achievement;
using UnityEngine;

namespace AI.Plan;

public class Investigate : GameAIState
{
	private const float RANGE = 0.5f;

	private const float RANGE_SQ = 0.25f;

	private const float MAX_MOVEMENT_DISTANCE_SQ = 9f;

	private PathingController m_pathingController = new PathingController();

	private PathToPosition.Params m_params = new PathToPosition.Params();

	private float m_waitTimer;

	private bool m_returnToPreviousLocation;

	protected TargetScanner m_targetScanner;

	protected Vector3 m_targetPos = Vector3.zero;

	protected Vector3 m_previousLocation = Vector3.zero;

	protected AttackBase m_attackToUse;

	public AttackBase Attack
	{
		get
		{
			return m_attackToUse;
		}
		set
		{
			m_attackToUse = value;
		}
	}

	public Vector3 TargetPos
	{
		get
		{
			return m_targetPos;
		}
		set
		{
			m_targetPos = value;
		}
	}

	public TargetScanner TargetScanner
	{
		get
		{
			return m_targetScanner;
		}
		set
		{
			m_targetScanner = value;
		}
	}

	public override void Reset()
	{
		base.Reset();
		m_params.Reset();
		m_pathingController.Reset();
		m_targetPos = Vector3.zero;
		m_previousLocation = Vector3.zero;
		m_returnToPreviousLocation = false;
		m_attackToUse = null;
		m_targetScanner = null;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		Mover mover = m_ai.Mover;
		if (mover != null)
		{
			mover.ClearForcedTurnDirection();
			mover.ReachedGoal = false;
			mover.IgnoreAttackBlocking = false;
			mover.enabled = true;
			m_previousLocation = mover.transform.position;
		}
		m_waitTimer = 3f;
		m_params.MovementType = AnimationController.MovementType.Walk;
		m_params.Range = 0.5f;
		m_params.Destination = m_targetPos;
		m_pathingController.Init(this, m_params, startPathing: true);
	}

	public override void Update()
	{
		base.Update();
		AIState currentState = base.Manager.CurrentState;
		if (m_targetScanner != null && m_targetScanner.ScanForTarget(m_owner, m_ai, -1f, ignoreIfCurrentTarget: true))
		{
			base.Manager.PopState(currentState);
		}
		else
		{
			if (base.Manager.CurrentState != this)
			{
				return;
			}
			if (m_returnToPreviousLocation)
			{
				if (m_pathingController.ReachedDestination())
				{
					m_pathingController.Stop();
					base.Manager.PopCurrentState();
				}
				else
				{
					m_pathingController.Update();
				}
				return;
			}
			Vector3 b = m_previousLocation;
			if (m_ai.Mover != null)
			{
				b = m_ai.Mover.transform.position;
			}
			if (m_pathingController.ReachedDestination() || GameUtilities.V3SqrDistance2D(m_previousLocation, b) > 9f)
			{
				m_pathingController.Stop();
				m_waitTimer -= Time.deltaTime;
				if (m_waitTimer <= 0f)
				{
					m_returnToPreviousLocation = true;
					Mover mover = m_ai.Mover;
					if (mover != null)
					{
						mover.ClearForcedTurnDirection();
						mover.ReachedGoal = false;
						mover.IgnoreAttackBlocking = false;
						mover.enabled = true;
					}
					m_params.Destination = m_previousLocation;
					m_pathingController.Init(this, m_params, startPathing: true);
				}
			}
			else
			{
				m_pathingController.Update();
			}
		}
	}

	public override bool IsMoving()
	{
		return m_pathingController.Enabled;
	}
}
