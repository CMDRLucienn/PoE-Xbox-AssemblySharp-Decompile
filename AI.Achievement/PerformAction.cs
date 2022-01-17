using UnityEngine;

namespace AI.Achievement;

public class PerformAction : GameAIState
{
	protected AnimationController.ActionType m_action;

	protected int m_variation = 1;

	protected bool m_waitForIdle;

	protected bool m_interrupted;

	protected float m_speed_multiplier = 1f;

	protected float m_loopTime;

	protected int m_loopCount;

	protected bool m_timeCalculated;

	protected TargetScanner m_targetScanner;

	public override bool CanCancel => false;

	public Renderer[] HiddenObjects { get; set; }

	public float RemainingTime => m_loopTime;

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

	public override bool UseQueuedTarget => true;

	public override void Reset()
	{
		base.Reset();
		m_action = AnimationController.ActionType.None;
		m_variation = 1;
		m_interrupted = false;
		m_speed_multiplier = 1f;
		m_loopTime = 0f;
		m_waitForIdle = false;
		m_loopCount = 0;
		m_timeCalculated = false;
		m_targetScanner = null;
		HiddenObjects = null;
	}

	public void InterruptAnimation()
	{
		if ((bool)m_animation)
		{
			m_animation.ClearActions();
			m_animation.DesiredAction.Reset();
			m_animation.Interrupt();
		}
		m_interrupted = true;
	}

	public void Setup(AnimationController.ActionType action, int variation)
	{
		m_action = action;
		m_variation = variation;
		m_loopCount = -1;
	}

	public void Setup(AnimationController.ActionType action, int variation, float loopTime)
	{
		m_action = action;
		m_variation = variation;
		m_loopTime = loopTime;
		m_loopCount = -1;
	}

	public void Setup(AnimationController.ActionType action, int variation, int loopCount)
	{
		m_action = action;
		m_variation = variation;
		m_loopCount = ((loopCount == 0) ? int.MaxValue : loopCount);
		m_loopTime = 1f;
		m_timeCalculated = false;
	}

	public void SetSpeedMultiplier(float speedMult)
	{
		m_speed_multiplier = speedMult;
	}

	public override void OnEnter()
	{
		if (m_animation == null)
		{
			m_animation = m_owner.GetComponent<AnimationController>();
		}
		if ((bool)m_animation)
		{
			m_animation.DesiredAction.m_actionType = m_action;
			m_animation.DesiredAction.m_variation = m_variation;
			m_animation.DesiredAction.m_speed *= m_speed_multiplier;
		}
		if (m_loopTime > 0f || m_loopCount >= 0)
		{
			m_animation.Loop = true;
		}
		if (HiddenObjects == null)
		{
			return;
		}
		for (int i = 0; i < HiddenObjects.Length; i++)
		{
			if ((bool)HiddenObjects[i])
			{
				HiddenObjects[i].enabled = false;
			}
		}
	}

	public override void OnExit()
	{
		m_loopTime = 0f;
		if ((bool)m_animation)
		{
			m_animation.Loop = false;
			m_animation.ClearActions();
			m_animation.DesiredAction.Reset();
		}
		RestoreHiddenObjects();
		HiddenObjects = null;
	}

	public override void OnAbort()
	{
		if (!m_animation.Idle)
		{
			m_animation.Interrupt();
		}
		m_loopTime = 0f;
		if ((bool)m_animation)
		{
			m_animation.Loop = false;
			m_animation.ClearActions();
			m_animation.DesiredAction.Reset();
		}
		RestoreHiddenObjects();
		HiddenObjects = null;
	}

	private void RestoreHiddenObjects()
	{
		if (HiddenObjects != null)
		{
			for (int i = 0; i < HiddenObjects.Length; i++)
			{
				if ((bool)HiddenObjects[i])
				{
					HiddenObjects[i].enabled = true;
				}
			}
		}
		if (m_parent != null && m_parent.AIController != null)
		{
			AlphaControl component = m_parent.AIController.gameObject.GetComponent<AlphaControl>();
			if (component != null)
			{
				component.Refresh();
			}
		}
	}

	public override void Interrupt()
	{
		InterruptAnimation();
	}

	private void ResetAnimation()
	{
		if ((bool)m_animation)
		{
			m_animation.DesiredAction.Reset();
		}
	}

	protected virtual void OnComplete()
	{
	}

	public override void Update()
	{
		if (m_interrupted)
		{
			if (m_animation.Idle)
			{
				base.Manager.PopCurrentState();
			}
		}
		else if (m_ai.IsConfused || (m_targetScanner != null && FogOfWar.PointVisibleInFog(m_ai.gameObject.transform.position) && m_targetScanner.ScanForTarget(m_owner, m_ai, -1f, ignoreIfCurrentTarget: true)))
		{
			Interrupt();
		}
		else
		{
			if (base.Manager.CurrentState != this)
			{
				return;
			}
			if (m_action == AnimationController.ActionType.Fidget || m_action == AnimationController.ActionType.Attack)
			{
				m_ai.UpdateEngagement(base.Owner, AIController.GetPrimaryAttack(base.Owner));
			}
			if (m_loopTime > 0f)
			{
				m_loopTime -= Time.deltaTime * m_speed_multiplier;
				if (m_loopCount > 0 && !m_timeCalculated && m_animation.IsPerformingAction(m_action, m_variation))
				{
					m_loopTime = 1f - m_loopTime;
					float lengthOfCurrentAnim = m_animation.GetLengthOfCurrentAnim();
					m_loopTime = lengthOfCurrentAnim * (float)m_loopCount - m_loopTime - lengthOfCurrentAnim * 0.5f;
					m_timeCalculated = true;
				}
				if (m_loopCount > 0 && m_loopTime <= 0f)
				{
					m_loopTime = 0f;
					m_animation.Loop = false;
				}
			}
			else if (m_waitForIdle && m_animation.Idle)
			{
				base.Manager.PopCurrentState();
			}
			else if (m_animation.CurrentAction.m_actionType != 0 && m_animation.CurrentAction.m_actionType != AnimationController.ActionType.Pending && !m_animation.IsPerformingAction(m_action, m_variation))
			{
				m_animation.DesiredAction.Reset();
				m_waitForIdle = true;
				OnComplete();
			}
		}
	}

	public override bool CanBeNudgedBy(Mover pather)
	{
		if (m_animation.CurrentAction.m_actionType == AnimationController.ActionType.Fidget || m_animation.CurrentAction.m_actionType == AnimationController.ActionType.None)
		{
			AIState queuedState = base.Manager.QueuedState;
			if (queuedState != null)
			{
				return queuedState.CanBeNudgedBy(pather);
			}
		}
		return false;
	}

	public override bool IsIdling()
	{
		return m_action == AnimationController.ActionType.Fidget;
	}
}
