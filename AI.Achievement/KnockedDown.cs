using UnityEngine;

namespace AI.Achievement;

public class KnockedDown : GameAIState, IStateWithDuration
{
	private const float WIND_UP_TIME = 0.2f;

	private const float KNOCK_DOWN_DURATION = 0.5f;

	private const float STAND_UP_DURATION = 1f;

	private float m_timer;

	private float m_windUpTimer;

	private bool m_standingUp;

	private bool m_inCombat = true;

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

	public override int Priority => 9;

	public override bool CanBeQueuedIfLowerPriority => false;

	public override bool UseQueuedTarget => true;

	public float Duration
	{
		get
		{
			return m_timer;
		}
		set
		{
			ResetKnockedDown(value);
		}
	}

	public override void Reset()
	{
		base.Reset();
		m_timer = 0f;
		m_windUpTimer = 0f;
		m_standingUp = false;
		m_inCombat = true;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		StopMover();
		m_ai.CancelAllEngagements();
		m_standingUp = false;
		m_animation.ClearReactions();
		m_animation.DesiredAction.Reset();
		m_animation.SetReaction(AnimationController.ReactionType.Knockdown);
	}

	public override void OnExit()
	{
		Standup();
	}

	public override void OnAbort()
	{
	}

	public override void Update()
	{
		if (!m_standingUp)
		{
			m_timer -= Time.deltaTime;
			if (m_timer <= 1f || !HasKnockedDownStatusEffect())
			{
				m_animation.SetReaction(AnimationController.ReactionType.Standup);
				m_animation.Loop = false;
				m_standingUp = true;
				m_windUpTimer = 0.2f;
			}
			else if (m_animation.CurrentReaction != AnimationController.ReactionType.Knockdown)
			{
				m_animation.ClearReactions();
				m_animation.DesiredAction.Reset();
				m_animation.SetReaction(AnimationController.ReactionType.Knockdown);
			}
		}
		else if (m_windUpTimer > 0f)
		{
			if (m_animation.CurrentReaction != AnimationController.ReactionType.Standup)
			{
				m_animation.ClearReactions();
				m_animation.DesiredAction.Reset();
				m_animation.SetReaction(AnimationController.ReactionType.Standup);
			}
			m_windUpTimer -= Time.deltaTime;
		}
		else if (m_animation.CurrentReaction != AnimationController.ReactionType.Standup)
		{
			m_animation.ClearReactions();
			base.Manager.PopCurrentState();
		}
	}

	private bool HasKnockedDownStatusEffect()
	{
		CharacterStats component = m_owner.GetComponent<CharacterStats>();
		if (component == null)
		{
			return false;
		}
		foreach (StatusEffect item in component.FindStatusEffectsOfType(StatusEffect.ModifiedStat.KnockedDown))
		{
			if (item.Duration > 0f)
			{
				return true;
			}
		}
		return false;
	}

	public void Standup()
	{
		m_timer = 0f;
		if (!m_standingUp)
		{
			m_animation.SetReaction(AnimationController.ReactionType.Standup);
			m_animation.Loop = false;
			m_standingUp = true;
			m_windUpTimer = 0.2f;
		}
	}

	public void SetKnockdownTime(float time)
	{
		m_timer = time;
	}

	public void ResetKnockedDown(float time)
	{
		if (time > m_timer)
		{
			m_timer = time;
		}
		if (m_standingUp)
		{
			m_standingUp = false;
			m_animation.ClearReactions();
			m_animation.DesiredAction.Reset();
			m_animation.SetReaction(AnimationController.ReactionType.Knockdown);
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
