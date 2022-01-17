using AI.Achievement;
using UnityEngine;

namespace AI.Plan;

public class Follow : GameAIState
{
	public const float FollowRange = 2f;

	public const float FollowRangeSq = 4f;

	private PathingController m_pathingController = new PathingController();

	private PathToPosition.Params m_params = new PathToPosition.Params();

	protected TargetScanner m_targetScanner;

	private Vector3 m_targetPos = Vector3.zero;

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
		}
		if (m_ai.Summoner == null)
		{
			m_ai.StateManager.PopCurrentState();
			return;
		}
		Mover component = m_ai.Summoner.GetComponent<Mover>();
		m_params.MovementType = AnimationController.MovementType.Walk;
		m_params.Range = 1.5f;
		m_params.Destination = component.Goal;
		m_targetPos = m_params.Destination;
		m_pathingController.Init(this, m_params, startPathing: true);
	}

	public override void OnExit()
	{
		base.OnExit();
		StopMover();
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
			if (m_pathingController.ReachedDestination())
			{
				m_pathingController.Stop();
				base.Manager.PopCurrentState();
			}
			else
			{
				if (!(m_ai.Summoner != null))
				{
					return;
				}
				Mover component = m_ai.Summoner.GetComponent<Mover>();
				float remainingPathDistance = component.GetRemainingPathDistance();
				float remainingPathDistance2 = m_ai.Mover.GetRemainingPathDistance();
				if (remainingPathDistance2 < float.Epsilon)
				{
					m_ai.Mover.RecalculatePath();
					remainingPathDistance2 = m_ai.Mover.GetRemainingPathDistance();
				}
				if (remainingPathDistance <= float.Epsilon || remainingPathDistance2 > remainingPathDistance + 2f)
				{
					if (!m_ai.Mover.enabled)
					{
						m_ai.Mover.enabled = true;
					}
					m_pathingController.Update();
					float num = GameUtilities.V3SqrDistance2D(m_ai.Mover.transform.position, component.transform.position);
					if (num > 49f)
					{
						m_ai.Mover.UseRunSpeed();
					}
					else if (num < 9f)
					{
						m_ai.Mover.UseWalkSpeed();
					}
				}
				else if (GameUtilities.V3SqrDistance2D(m_targetPos, component.Goal) > float.Epsilon)
				{
					if (component.Goal.sqrMagnitude > float.Epsilon)
					{
						m_targetPos = component.Goal;
					}
					else
					{
						m_targetPos = component.transform.position;
					}
					m_params.Destination = m_targetPos;
					m_pathingController.Init(this, m_params, startPathing: true);
				}
				else
				{
					m_ai.Mover.Stop();
					m_ai.Mover.enabled = false;
					m_animation.ClearActions();
				}
			}
		}
	}

	public override bool IsMoving()
	{
		return m_pathingController.Enabled;
	}
}
