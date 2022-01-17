using System.Collections.Generic;

namespace AI.Achievement;

public class Patrol : GameAIState
{
	private enum PatrolState
	{
		NotSet,
		Moving,
		Stationary
	}

	protected TargetScanner m_targetScanner;

	private PathingController m_pathingController = new PathingController();

	private PathToPosition.Params m_params = new PathToPosition.Params();

	private AIPackageController m_packageController;

	private PatrolState m_patrolState;

	private Waypoint m_startingWaypoint;

	private bool m_reachedLastWaypoint;

	public Waypoint StartPoint
	{
		get
		{
			return m_startingWaypoint;
		}
		set
		{
			m_startingWaypoint = value;
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
		m_startingWaypoint = null;
		m_reachedLastWaypoint = false;
		m_targetScanner = null;
		m_packageController = null;
		m_patrolState = PatrolState.NotSet;
		m_params.Reset();
		m_pathingController.Reset();
	}

	public override void OnEnter()
	{
		base.OnEnter();
		if (m_targetScanner == null)
		{
			m_targetScanner = new TargetScanner();
		}
		if (!m_animation.Idle || m_animation.DesiredAction.m_actionType != 0)
		{
			m_animation.Interrupt();
		}
		m_animation.ClearReactions();
		m_animation.ClearActions();
		m_animation.DesiredAction.Reset();
		bool startPathing = true;
		if (!m_reachedLastWaypoint)
		{
			List<Waypoint> s_ActiveWayPoints = Waypoint.s_ActiveWayPoints;
			Waypoint waypoint = m_startingWaypoint;
			if (waypoint == null)
			{
				float num = float.MaxValue;
				foreach (Waypoint item in s_ActiveWayPoints)
				{
					float num2 = GameUtilities.V3SqrDistance2D(item.transform.position, m_owner.transform.position);
					if (num2 < num)
					{
						num = num2;
						waypoint = item;
					}
				}
			}
			m_startingWaypoint = waypoint;
			if (m_ai.PrevWaypoint == null)
			{
				m_ai.PrevWaypoint = waypoint;
			}
			if (m_ai.CurrentWaypoint == null)
			{
				if (m_ai.PrevWaypoint != null)
				{
					m_ai.CurrentWaypoint = m_ai.PrevWaypoint;
				}
				else
				{
					m_ai.CurrentWaypoint = waypoint;
				}
				if (GameUtilities.V3SqrDistance2D(m_ai.CurrentWaypoint.transform.position, m_owner.transform.position) < 0.25f && m_ai.CurrentWaypoint.NextWaypoint != null)
				{
					m_ai.CurrentWaypoint = m_ai.CurrentWaypoint.NextWaypoint;
				}
			}
			if (m_ai.CurrentWaypoint == null)
			{
				base.Manager.PopCurrentState();
				return;
			}
			m_params.Range = 0.15f;
			m_params.Destination = m_ai.CurrentWaypoint.transform.position;
			if (m_ai.CurrentWaypoint.WalkOnly)
			{
				m_params.MovementType = AnimationController.MovementType.Walk;
			}
			else
			{
				m_params.MovementType = AnimationController.MovementType.Run;
			}
		}
		else if (m_ai.CurrentWaypoint == null)
		{
			base.Manager.PopCurrentState();
			return;
		}
		m_packageController = m_owner.GetComponent<AIPackageController>();
		Mover mover = m_ai.Mover;
		if (mover != null)
		{
			mover.ClearForcedTurnDirection();
			mover.ReachedGoal = false;
			mover.IgnoreAttackBlocking = false;
			if (m_packageController != null && m_packageController.Patroller)
			{
				mover.enabled = true;
			}
			else
			{
				startPathing = false;
			}
		}
		Equipment component = m_owner.GetComponent<Equipment>();
		if (component != null)
		{
			component.SelectWeaponSet(0, enforceRecoveryPenalty: false);
		}
		if (m_ai.Mover != null)
		{
			m_pathingController.Init(this, m_params, startPathing);
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		m_patrolState = PatrolState.NotSet;
		StopMover();
	}

	public override void Update()
	{
		base.Update();
		if (m_ai == null)
		{
			return;
		}
		Mover mover = m_ai.Mover;
		if (m_packageController != null && (m_patrolState == PatrolState.NotSet || (m_patrolState == PatrolState.Stationary && m_packageController.Patroller) || (m_patrolState == PatrolState.Moving && !m_packageController.Patroller)))
		{
			if (m_packageController.Patroller)
			{
				m_patrolState = PatrolState.Moving;
				if (mover != null)
				{
					mover.enabled = true;
					m_pathingController.Init(this, m_params, startPathing: true);
				}
			}
			else
			{
				m_patrolState = PatrolState.Stationary;
				StopMover();
			}
		}
		AIState currentState = base.Manager.CurrentState;
		if (m_targetScanner != null && FogOfWar.PointVisibleInFog(m_ai.gameObject.transform.position) && m_targetScanner.ScanForTarget(m_owner, m_ai, -1f, ignoreIfCurrentTarget: false))
		{
			base.Manager.PopState(currentState);
		}
		else
		{
			if (base.Manager.CurrentState != this)
			{
				return;
			}
			if (m_ai.CurrentWaypoint == null)
			{
				StopMover();
				base.Manager.PopCurrentState();
				return;
			}
			if (m_packageController == null)
			{
				if (mover != null && mover.enabled)
				{
					mover.Stop();
				}
				return;
			}
			m_pathingController.Update();
			if (m_pathingController.ReachedDestination())
			{
				ScriptEvent scriptEvent = null;
				if (m_ai.CurrentWaypoint != null)
				{
					scriptEvent = m_ai.CurrentWaypoint.GetComponent<ScriptEvent>();
				}
				if ((bool)scriptEvent)
				{
					scriptEvent.ExecuteScript(ScriptEvent.ScriptEvents.OnArrive);
					if (m_ai == null)
					{
						return;
					}
				}
				if (m_ai.CurrentWaypoint != null && m_ai.CurrentWaypoint.PauseTime > 0f)
				{
					Wait wait = PushState<Wait>();
					wait.Duration = m_ai.CurrentWaypoint.PauseTime;
					wait.ForwardFacing = m_ai.CurrentWaypoint.transform.forward;
					if (m_ai.CurrentWaypoint.NextWaypoint == null)
					{
						wait.WaitForFidgetToFinish = true;
						if (mover != null && !mover.PositionOverlaps(m_ai.CurrentWaypoint.transform.position, mover))
						{
							m_owner.transform.position = m_ai.CurrentWaypoint.transform.position;
						}
						m_reachedLastWaypoint = true;
					}
					if (m_ai.CurrentWaypoint.AmbientAnimation != 0)
					{
						if (m_ai.CurrentWaypoint.LoopCount >= 0)
						{
							wait.Setup(AnimationController.ActionType.Ambient, m_ai.CurrentWaypoint.AmbientVariation, m_ai.CurrentWaypoint.LoopCount);
						}
						else
						{
							wait.Setup(AnimationController.ActionType.Ambient, m_ai.CurrentWaypoint.AmbientVariation);
						}
					}
					else
					{
						wait.Setup(AnimationController.ActionType.Fidget, OEIRandom.Index(m_animation.FidgetCount) + 1);
					}
				}
				m_ai.PrevWaypoint = m_ai.CurrentWaypoint;
				if (m_ai.PrevWaypoint != null)
				{
					m_ai.CurrentWaypoint = m_ai.PrevWaypoint.NextWaypoint;
				}
				else
				{
					m_ai.CurrentWaypoint = null;
				}
				if (m_ai.CurrentWaypoint != null)
				{
					m_ai.RecordRetreatPosition(m_ai.CurrentWaypoint.transform.position);
					m_params.Target = m_ai.CurrentWaypoint.gameObject;
					if (m_ai.CurrentWaypoint.WalkOnly)
					{
						m_params.MovementType = AnimationController.MovementType.Walk;
						mover.UseWalkSpeed();
					}
					else
					{
						m_params.MovementType = AnimationController.MovementType.Run;
						mover.UseRunSpeed();
					}
				}
				else
				{
					m_reachedLastWaypoint = true;
				}
			}
			else if (m_patrolState == PatrolState.Moving && !m_pathingController.Mover.HasGoal)
			{
				if (mover != null)
				{
					mover.ClearForcedTurnDirection();
					mover.ReachedGoal = false;
					mover.enabled = true;
				}
				m_pathingController.Init(this, m_params, startPathing: true);
			}
			if (mover != null && !mover.enabled)
			{
				m_ai.MoveToRetreatPosition();
			}
		}
	}

	public override string GetDebugText()
	{
		if (m_ai.CurrentWaypoint != null)
		{
			return ": Current Waypoint: " + m_ai.CurrentWaypoint.ToString();
		}
		return string.Empty;
	}

	public override bool IsMoving()
	{
		return m_ai.Mover.Speed > float.Epsilon;
	}
}
