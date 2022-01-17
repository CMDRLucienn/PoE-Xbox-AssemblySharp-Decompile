using UnityEngine;

namespace AI.Plan;

public class Ability : GameAIState
{
	public GenericAbility QueuedAbility;

	public GameObject Target;

	private AIController m_aiController;

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

	public override GameObject CurrentTarget => Target;

	public override void Reset()
	{
		base.Reset();
		QueuedAbility = null;
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_aiController = GameUtilities.FindActiveAIController(m_owner);
		if (QueuedAbility == null || m_aiController == null)
		{
			base.Manager.PopCurrentState();
		}
	}

	public override void Update()
	{
		base.Update();
		if (QueuedAbility == null)
		{
			base.Manager.PopCurrentState();
		}
		else if (QueuedAbility.Ready)
		{
			base.Manager.PopCurrentState();
			QueuedAbility.Activate(Target);
		}
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
