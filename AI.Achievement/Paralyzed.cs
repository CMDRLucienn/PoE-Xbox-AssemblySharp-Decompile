using UnityEngine;

namespace AI.Achievement;

public class Paralyzed : GameAIState, IStateWithDuration
{
	private float m_timer;

	private bool m_inCombat = true;

	public float Duration
	{
		get
		{
			return m_timer;
		}
		set
		{
			m_timer = value;
		}
	}

	public bool InCombatOverride
	{
		get
		{
			return m_inCombat;
		}
		set
		{
			m_inCombat = value;
		}
	}

	public override bool InCombat => m_inCombat;

	public override int Priority => 7;

	public override bool CanBeQueuedIfLowerPriority => false;

	public override bool UseQueuedTarget => true;

	public override void Reset()
	{
		base.Reset();
		Duration = 0f;
		m_timer = 0f;
		m_inCombat = true;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_animation.TimeScaleOverride = 1;
		m_ai.Mover.Frozen = true;
	}

	public override void OnExit()
	{
		base.OnExit();
		m_animation.TimeScaleOverride = 0;
		m_ai.Mover.Frozen = false;
	}

	public override void Update()
	{
		m_timer -= Time.deltaTime;
		if (m_timer < float.Epsilon)
		{
			m_ai.StateManager.PopCurrentState();
		}
	}

	public override bool AllowEngagementUpdate()
	{
		return false;
	}

	public override bool TurnWhilePaused()
	{
		return false;
	}
}
