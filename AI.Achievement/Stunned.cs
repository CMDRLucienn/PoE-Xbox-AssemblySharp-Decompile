namespace AI.Achievement;

public class Stunned : PerformReaction
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

	public override int Priority => 5;

	public override bool CanBeQueuedIfLowerPriority => false;

	public override bool UseQueuedTarget => true;

	public override void Reset()
	{
		base.Reset();
		m_inCombat = true;
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
