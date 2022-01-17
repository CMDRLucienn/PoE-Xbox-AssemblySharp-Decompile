namespace AI.Player;

public class Ability : PlayerState
{
	public GenericAbility QueuedAbility;

	public override GenericAbility CurrentAbility
	{
		get
		{
			if ((bool)QueuedAbility)
			{
				return QueuedAbility;
			}
			return base.CurrentAbility;
		}
	}

	public override void Reset()
	{
		base.Reset();
		QueuedAbility = null;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		if (m_partyMemberAI != null && QueuedAbility != null && !QueuedAbility.Activated)
		{
			if (QueuedAbility.UsePrimaryAttack || QueuedAbility.UseFullAttack)
			{
				if (m_ai.StateManager.QueuedState is Attack attack)
				{
					attack.Ability = QueuedAbility;
				}
			}
			else
			{
				m_partyMemberAI.QueuedAbility = QueuedAbility;
			}
		}
		base.Manager.PopCurrentState();
	}

	public override string GetDebugText()
	{
		if (QueuedAbility != null)
		{
			return ": " + QueuedAbility.ToString();
		}
		return string.Empty;
	}
}
