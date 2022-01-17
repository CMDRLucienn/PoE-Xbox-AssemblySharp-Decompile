using AI.Achievement;
using UnityEngine;

public class FidgetController
{
	private bool m_enabled = true;

	private float m_timeToFidget;

	private AIState m_state;

	private AnimationController m_animation;

	private TargetScanner m_targetScanner;

	private const float COMBAT_MODIFIER = 0.3333f;

	public bool Enabled
	{
		get
		{
			return m_enabled;
		}
		set
		{
			m_enabled = value;
		}
	}

	public void Reset()
	{
		m_state = null;
		m_animation = null;
		m_targetScanner = null;
	}

	public void Init(AIState state, AnimationController animation, TargetScanner targetScanner)
	{
		m_state = state;
		m_animation = animation;
		m_targetScanner = targetScanner;
		m_timeToFidget = Random.Range(m_animation.MinFidgetTime * 0.95f, m_animation.MinFidgetTime * 2.5f);
		if (GameState.InCombat)
		{
			m_timeToFidget *= 0.3333f;
		}
	}

	public void Update()
	{
		if (!Enabled || m_animation == null || m_state == null)
		{
			return;
		}
		if (m_timeToFidget > 0f)
		{
			m_timeToFidget -= Time.deltaTime;
			return;
		}
		m_timeToFidget = Random.Range(m_animation.MinFidgetTime * 0.95f, m_animation.MinFidgetTime * 1.5f);
		if (GameState.InCombat)
		{
			m_timeToFidget *= 0.3333f;
		}
		PerformAction performAction = m_state.PushState<PerformAction>();
		performAction.Setup(AnimationController.ActionType.Fidget, OEIRandom.Index(m_animation.FidgetCount) + 1);
		performAction.TargetScanner = m_targetScanner;
	}
}
