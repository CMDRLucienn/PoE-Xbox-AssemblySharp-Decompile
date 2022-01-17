using UnityEngine;

namespace AI.Achievement;

public class PerformReaction : GameAIState, IStateWithDuration
{
	private float m_timer;

	private AnimationController.ReactionType m_reaction;

	private bool m_waitForEnd;

	private bool m_animStarted;

	public AnimationController.ReactionType Reaction
	{
		get
		{
			return m_reaction;
		}
		set
		{
			m_reaction = value;
		}
	}

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

	public override bool InCombat
	{
		get
		{
			if (m_reaction == AnimationController.ReactionType.Standup)
			{
				if (GameState.InCombat)
				{
					return !GameState.IsInTrapTriggeredCombat;
				}
				return false;
			}
			return true;
		}
	}

	public override bool UseQueuedTarget => true;

	public override void Reset()
	{
		base.Reset();
		m_timer = 0f;
		m_reaction = AnimationController.ReactionType.None;
		m_waitForEnd = false;
		m_animStarted = false;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_animation.SetReaction(m_reaction);
		StopMover();
		m_ai.CancelAllEngagements();
	}

	public override void OnExit()
	{
		InterruptAnimation();
	}

	public override void OnAbort()
	{
		InterruptAnimation();
	}

	private void InterruptAnimation()
	{
		if ((bool)m_animation)
		{
			m_animation.SetReaction(AnimationController.ReactionType.None);
			m_animation.ClearReactions();
			if (!m_animation.Idle)
			{
				m_animation.Interrupt();
			}
			m_animation.Loop = false;
			m_animation.ClearActions();
			m_animation.DesiredAction.Reset();
		}
	}

	public override void Update()
	{
		if (!m_waitForEnd && m_timer > 0f)
		{
			m_animation.Loop = true;
			m_timer -= Time.deltaTime;
			if (m_timer <= 0f)
			{
				ClearReaction();
			}
			return;
		}
		m_animation.Loop = false;
		if (m_waitForEnd && !m_animStarted)
		{
			m_animStarted = m_animation.IsPerformingReaction(m_reaction);
			return;
		}
		AnimationController.ReactionType currentReaction = m_animation.CurrentReaction;
		if (currentReaction == m_reaction)
		{
			if (!m_animation.IsPerformingReaction(m_reaction))
			{
				InterruptAnimation();
				base.Manager.PopCurrentState();
			}
		}
		else if (!m_waitForEnd && currentReaction == AnimationController.ReactionType.None)
		{
			InterruptAnimation();
			base.Manager.PopCurrentState();
		}
		else if (m_animStarted && currentReaction == AnimationController.ReactionType.None)
		{
			InterruptAnimation();
			base.Manager.PopCurrentState();
		}
	}

	public void Setup(AnimationController.ReactionType reaction)
	{
		Reaction = reaction;
		Duration = 0f;
		m_waitForEnd = true;
	}

	public void Setup(AnimationController.ReactionType reaction, float time)
	{
		Reaction = reaction;
		Duration = time;
	}

	public void ClearReaction()
	{
		if ((bool)m_animation)
		{
			m_animation.ClearReactions();
			m_animation.DesiredAction.Reset();
		}
		m_timer = 0f;
	}

	public override void OnEvent(GameEventArgs args)
	{
		base.OnEvent(args);
		GameEventType type = args.Type;
		if (type == GameEventType.Stunned && args.IntData[0] == 0 && m_animation.CurrentReaction != AnimationController.ReactionType.Stun)
		{
			ClearReaction();
		}
	}
}
