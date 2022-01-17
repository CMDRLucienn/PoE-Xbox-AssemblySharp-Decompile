using UnityEngine;

namespace AI.Achievement;

public class Wait : PerformAction
{
	private float m_timer;

	public float Duration { get; set; }

	public bool InfiniteWait { get; set; }

	public bool WaitForFidgetToFinish { get; set; }

	public Vector3 ForwardFacing { get; set; }

	public bool FlagTurnWhilePaused { get; set; }

	public override void Reset()
	{
		base.Reset();
		Duration = 0f;
		InfiniteWait = false;
		WaitForFidgetToFinish = false;
		FlagTurnWhilePaused = false;
		m_timer = 0f;
		m_interrupted = false;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		if (Duration == 0f)
		{
			Duration = 1f;
		}
		StopMover();
		if (m_targetScanner == null)
		{
			m_targetScanner = m_ai.GetTargetScanner();
		}
	}

	public override void Update()
	{
		if (m_interrupted)
		{
			if (m_animation.Idle)
			{
				base.Manager.PopCurrentState();
			}
			return;
		}
		if (!m_ai.IsConfused && m_targetScanner != null && FogOfWar.PointVisibleInFog(m_ai.gameObject.transform.position) && m_targetScanner.ScanForTarget(m_owner, m_ai, -1f, ignoreIfCurrentTarget: true))
		{
			m_timer = Duration;
			InterruptAnimation();
		}
		if (base.Manager.CurrentState != this)
		{
			return;
		}
		if (!InfiniteWait)
		{
			m_timer += Time.deltaTime;
			if (m_timer >= Duration)
			{
				if (WaitForFidgetToFinish)
				{
					if (m_ai.CurrentWaypoint != null && m_ai.StateManager.QueuedState is Patrol)
					{
						m_animation.Loop = false;
					}
				}
				else
				{
					InterruptAnimation();
				}
			}
		}
		if (!FlagTurnWhilePaused && ForwardFacing.sqrMagnitude > float.Epsilon)
		{
			m_ai.FaceDirection(ForwardFacing);
		}
		if (m_waitForIdle && m_animation.Idle)
		{
			base.Manager.PopCurrentState();
		}
		else if (m_animation.CurrentAction.m_actionType != 0 && m_animation.CurrentAction.m_actionType != AnimationController.ActionType.Pending && !m_animation.IsPerformingAction(m_action, m_variation) && (m_interrupted || (!InfiniteWait && !(m_timer <= Duration))))
		{
			m_waitForIdle = true;
		}
	}

	public override void UpdateWhenPaused()
	{
		if (FlagTurnWhilePaused && ForwardFacing.sqrMagnitude > float.Epsilon)
		{
			m_ai.FaceDirection(ForwardFacing);
		}
		base.UpdateWhenPaused();
	}
}
