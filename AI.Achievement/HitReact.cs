namespace AI.Achievement;

public class HitReact : GameAIState
{
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

	public override int Priority => 4;

	public override bool CanBeQueuedIfLowerPriority => false;

	public override bool UseQueuedTarget => true;

	public override void Reset()
	{
		base.Reset();
		m_inCombat = true;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_animation.DesiredAction.Reset();
		m_animation.Loop = false;
		m_animation.SetReaction(AnimationController.ReactionType.Hit);
		StopMover();
	}

	public override void OnExit()
	{
	}

	public override void Update()
	{
		if (m_animation.CurrentReaction != AnimationController.ReactionType.Hit)
		{
			base.Manager.PopCurrentState();
		}
	}
}
